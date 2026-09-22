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
        private Thread? _ServerThread = null;
        private int? _ExitCodeOfServer = null;
        private Exception? _ExceptionOfServer = null;
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
                    this._ExitCodeOfServer = this._Program.MainImplementation(args);
                }
                catch (Exception exception)
                {
                    //the exception must not leave this thread: an unhandled exception of a thread terminates the entire test-process, which hides which testcase failed and why. It is reported by EnsureServerIsStopped() instead.
                    this._ExceptionOfServer = exception;
                }
            };
            if (this._IntegrationTestConfiguration.RunInOwnThread)
            {
                this._ServerThread = new Thread(() => action())
                {
                    Name = nameof(Program),
                    //the thread must not keep the test-process alive on its own: the test-runner collects the results (for example the test-coverage, which the process writes when it ends) directly after the last testcase, so a thread which is still running at that moment would be too late. The thread is awaited explicitly when the server is stopped.
                    IsBackground = true
                };
                this._ServerThread.Start();
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
                    this.EnsureServerIsStillRunning();
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
            this._BusinessLogicService = GRYLibrary.Core.Misc.Utilities.GetValue(program._BusinessLogicService, nameof(Program._BusinessLogicService));
            this._Log = program._Log;
        }

        /// <summary>Reports a server which already ended, so that a server which can not start is reported immediately instead of after the timeout of the wait for its availability.</summary>
        private void EnsureServerIsStillRunning()
        {
            if (this._ExceptionOfServer != null)
            {
                throw new DependencyNotAvailableException("The server stopped with an exception.", this._ExceptionOfServer);
            }
            if (this._ExitCodeOfServer.HasValue)
            {
                throw new DependencyNotAvailableException(this.GetExitCodeMessage());
            }
        }

        /// <summary>Describes the exit-code of the server together with its last log-entries.</summary>
        private string GetExitCodeMessage()
        {
            string message = $"Exitcode of main-method was {this._ExitCodeOfServer}.";
            if (this._Program != null && this._Program._Log != null)//TODO this condition should not be required. but for unknown reasons the _log-property is null and this causes problems whille retrieving the logs here which would be useful.
            {
                IGRYLog log = this._Program._Log;
                LogItem[] logMessages = log.LastLogEntries.GetEntries();
                if (logMessages.Any())
                {
                    message = $"{message} Last log entries:\n" + string.Join("\n", logMessages.Select(item =>
                    {
                        item.Format(log.Configuration, out string result, out int _, out int _, out ConsoleColor _, GRYLogLogFormat.GRYLogFormat);
                        return result;
                    }));
                }
            }
            return message;
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
                if (this._ServerThread != null)
                {
                    //the server-thread is awaited so that the testcase leaves no running thread behind.
                    GRYLibrary.Core.Misc.Utilities.AssertCondition(this._ServerThread.Join(TimeSpan.FromSeconds(60)), () => "The thread of the server did not end.");
                    this._ServerThread = null;
                }
                if (this._ExceptionOfServer != null)
                {
                    throw new DependencyNotAvailableException("The server stopped with an exception.", this._ExceptionOfServer);
                }
                GRYLibrary.Core.Misc.Utilities.AssertCondition(this._ExitCodeOfServer == 0, this.GetExitCodeMessage);
            }
        }
    }
}
