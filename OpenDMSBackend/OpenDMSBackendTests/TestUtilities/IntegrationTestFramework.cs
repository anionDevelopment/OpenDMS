using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities.InitializationStates;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class IntegrationTestFramework : IDisposable
    {
        private bool _Running
        {
            get
            {
                if (this._Program == null)
                {
                    return false;
                }
                return this._Program.IsRunning;
            }
        }
        private readonly IDictionary<User, string> _UserPasswords = new Dictionary<User, string>();
        private Program? _Program = null;
        private readonly IntegrationTestConfiguration _IntegrationTestConfiguration;
        internal IBusinessLogicService? _BusinessLogicService;
        internal IGRYLog? _Log;
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

                    string[] args = Array.Empty<string>();
                    int exitCode = this._Program.MainImplementation(args);
                    GRYLibrary.Core.Misc.Utilities.AssertCondition(exitCode == 0, () =>
                    {
                        string message = $"Exitode of main-method was {exitCode}.";
                        if (this._Program._Log != null)//TODO this condition should not be required. but for unknown reasons the _log-property is null and this causes problems whille retrieving the logs here which would be useful.
                        {
                            IGRYLog log = GRYLibrary.Core.Misc.Utilities.AssertNotNull(this._Program._Log, nameof(Program._Log));
                            LogItem[] logMessages = log.LastLogEntries.GetEntries();
                            if (logMessages.Any())
                            {
                                message = $"{message} Last log entries:\n" + string.Join("\n", logMessages.Select(item =>
                                {
                                    item.Format(this._Program._Log.Configuration, out string result, out int _, out int _, out ConsoleColor _, GRYLogLogFormat.GRYLogFormat);
                                    return result;
                                }));
                            }
                        }
                        return message;
                    });
                }
                catch
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
            }, TimeSpan.FromSeconds(120)))
            {
                throw new Exception("Could not start service.");
            }
            this._BusinessLogicService = this._Program._BusinessLogicService;
            this._Log = this._Program._Log;
        }

        private bool IsReady()
        {
            try
            {
                using HttpClient client = this.GetClient();
                string url = $"{this.GetServerURL()}{ServerConfiguration.APIRoutePrefix}/Other/Maintenance/InitializationState";
                HttpResponseMessage response = client.GetAsync(url).WaitAndGetResult();
                Assert.IsTrue(response.IsSuccessStatusCode);
                string content = response.Content.ReadAsStringAsync().WaitAndGetResult();
                GRYLibrary.Core.Misc.Utilities.AssertCondition(content != typeof(InitializationFailed).Name);
                return content == typeof(Initialized).Name;
            }
            catch
            {
                return false;
            }
        }

        public HttpClient GetClient(User? user = null)
        {
            HttpClient result = new HttpClient();
            if (user != null)
            {
                result.DefaultRequestHeaders.Add("X-Accesstoken", this._BusinessLogicService.Login(user.Name, this._UserPasswords[user]).Value);
            }
            return result;
        }
        public User GetUser()
        {
            string username = Guid.NewGuid().ToString();
            string password = Guid.NewGuid().ToString();
            string userId = this._BusinessLogicService.Register(username, password);
            User user = this._BusinessLogicService.GetUser(userId);
            this._UserPasswords[user] = password;
            return user;
        }
        public string GetServerURL()
        {
            return $"http://127.0.0.1:{HTTP.DefaultPort}";
        }
        public void Dispose()
        {
            this.EnsureServerIsStopped();
        }

        private void EnsureServerIsStopped()
        {
            if (this._Running)
            {
                try
                {
                    this._Program.Stop();
                    GRYLibrary.Core.Misc.Utilities.WaitUntilConditionIsTrue(() => !this._Program.IsRunning);
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}
