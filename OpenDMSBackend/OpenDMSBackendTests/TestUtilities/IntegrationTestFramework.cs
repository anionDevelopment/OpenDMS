using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OpenDMSBackend.Core;
using OpenDMSBackend.Core.Configuration;
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
        private bool Started = false;
        private readonly IDictionary<User, string> _UserPasswords = new Dictionary<User, string>();
        private Program? _Program = null;
        private readonly IntegrationTestConfiguration _IntegrationTestConfiguration;
        internal IBusinessLogicService? _BusinessLogicService;
        internal IGRYLog? _Log;
        public IntegrationTestFramework(bool startServer) : this(new IntegrationTestConfiguration(), startServer)
        {
        }
        public IntegrationTestFramework(IntegrationTestConfiguration integrationTestConfiguration, bool startServer)
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
                    this._Program = new Program
                    {
                        RunAsync = !this._IntegrationTestConfiguration.RunInOwnThread,
                        ListenOnEveryIP = false,
                        SetupMocks = this._IntegrationTestConfiguration.SetupMocks
                    };

                    string[] args = new string[] {
                        $"--{nameof(CommandlineParameter.UseMockOCRService)}"
                    };//TODO add option to pass more configuration-values for the test-run like port etc. so that this can not go wrong due to a different configuration from a previous (manual) run.
                    int exitCode = this._Program.MainImplementation(args);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
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
                catch (Exception ex)
                {
                    throw;
                }
            };
            if (this._IntegrationTestConfiguration.RunInOwnThread)
            {
                Thread t = new Thread(() => action())
                {
                    Name = nameof(Program)
                };
                t.Start();
            }
            else
            {
                action();
            }
            Exception? lastException = null;
            if (!GRYLibrary.Core.Misc.Utilities.RunWithTimeout(() =>
            {
                while (!this.IsReady(out lastException))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }, TimeSpan.FromSeconds(120)))
            {
                if (lastException == null)
                {
                    throw new DependencyNotAvailableException("Could not start service.");
                }
                else
                {
                    throw lastException;
                }
            }
            var program = GRYLibrary.Core.Misc.Utilities.AssertNotNull(this._Program, nameof(this._Program));
            this.Started = true;
            this._BusinessLogicService =GRYLibrary.Core.Misc.Utilities.GetValue( program._BusinessLogicService,nameof(Program._BusinessLogicService));
            this._Log = program._Log;
        }

        private bool IsReady(out Exception? exception)
        {
            try
            {
                using HttpClient client = this.GetClient();
                string url = $"{this.GetServerURL()}{ServerConfiguration.APIRoutePrefix}/Other/Maintenance/HealthCheck";
                HttpResponseMessage response = client.GetAsync(url).WaitAndGetResult();
                Assert.IsTrue(response.IsSuccessStatusCode);
                string content = response.Content.ReadAsStringAsync().WaitAndGetResult();
                dynamic obj = JsonConvert.DeserializeObject(content)!;
                int status = (int)obj["status"];
                exception = null;
                if (status == 2) //2 means healthy.
                {
                    exception = null;
                    return true;
                }
                else
                {
                    exception = new NotReadyException($"Service is not healthy yet due to status \"{status}\".");
                    return false;
                }
            }
            catch (Exception e)
            {
                exception = e;
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
            var businessLogicService = GRYLibrary.Core.Misc.Utilities.AssertNotNull(this._BusinessLogicService, nameof(this._BusinessLogicService));
            string userId = businessLogicService.Register(username, password);
            User user = businessLogicService.GetUser(userId);
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
            if (this.Started)
            {
                this._Program.Stop();
                this.Started = false;
            }
        }
    }
}
