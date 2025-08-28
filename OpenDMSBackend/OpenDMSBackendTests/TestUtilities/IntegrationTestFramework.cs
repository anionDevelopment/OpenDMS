using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.MetaConfiguration.ConfigurationFormats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OpenDMSBackend.Core;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public class IntegrationTestFramework : IDisposable
    {
        public bool Started { get; private set; }
        private IDictionary<User, string> UserPasswords = new Dictionary<User, string>();
        private Program _Program;
        private readonly IntegrationTestConfiguration _IntegrationTestConfiguration;
        public IBusinessLogicService BusinessLogicService { get; private set; }
        public IntegrationTestFramework(bool startServer = true) : this(new IntegrationTestConfiguration(), startServer)
        {
        }
        public IntegrationTestFramework(IntegrationTestConfiguration integrationTestConfiguration, bool startServer = true)
        {
            this._IntegrationTestConfiguration = integrationTestConfiguration;
            if (startServer)
            {
                this.StartServer();
            }
        }
        public void StartServer()
        {
            Action action = () =>
            {
                try
                {

                this._Program = new Program();
                this._Program.RunAsync = !this._IntegrationTestConfiguration.RunInOwnThread;
                this._Program.ListenOnEveryIP = false;
                this._Program.SetupMocks = this._IntegrationTestConfiguration.SetupMocks;
                string[] args = new string[] { 
                    @$"--{nameof(CommandlineParameter.TestRun)}",
                    @$"--{nameof(CommandlineParameter.OCRDataFolder)}", Utilities.GetOCRDataFolder()
                };
                GRYLibrary.Core.Misc.Utilities.AssertCondition(this._Program.MainImplementation(args) == 0, "Exitode of main-method was non-zero.");
                this.BusinessLogicService = this._Program.BusinessLogicService;
                }
                catch (Exception ex)
                {
                    throw;
                }
            };
            if (this._IntegrationTestConfiguration.RunInOwnThread)
            {
                Thread t = new Thread(() => action());
                t.Start();
            }
            else
            {
                action();
            }
            if (!GRYLibrary.Core.Misc.Utilities.RunWithTimeout(() =>
            {
                while (!this.IsReady())
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }, TimeSpan.FromSeconds(150)))
            {
                throw new Exception("Could not start service.");
            }
            this.Started = true;
        }

        private bool IsReady()
        {
            try
            {
                using HttpClient client = this.GetClient();
                string url = $"{this.GetServerURL()}{ServerConfiguration.APIRoutePrefix}/Other/Maintenance/HealthCheck";
                HttpResponseMessage response = client.GetAsync(url).WaitAndGetResult();
                Assert.IsTrue(response.IsSuccessStatusCode);
                var content = response.Content.ReadAsStringAsync().WaitAndGetResult();
                dynamic obj = JsonConvert.DeserializeObject(content);
                int status = (int)obj["status"];
                return status == 2;//2 means healthy.
            }
            catch
            {
                return false;
            }
            return true;
        }

        public HttpClient GetClient(User? user = null)
        {
            HttpClient result = new HttpClient();
            if (user != null)
            {
                result.DefaultRequestHeaders.Add("X-Accesstoken", this.BusinessLogicService.Login(user.Name, this.UserPasswords[user]).Value);
            }
            return result;
        }
        public User GetUser()
        {
            string username = Guid.NewGuid().ToString();
            string password = Guid.NewGuid().ToString();
            string userId = this.BusinessLogicService.Register(username, password);
            User user = this.BusinessLogicService.GetUser(userId);
            this.UserPasswords[user] = password;
            return user;
        }
        public string GetServerURL()
        {
            return $"http://127.0.0.1:{CodeUnitSpecificConstants.PortForTestRun}";
        }
        public void Dispose()
        {
            this.EnsureServerIsStopped();
        }

        private void EnsureServerIsStopped()
        {
            if (this.Started)
            {
                this._Program.Stop();
            }
        }
    }
}
