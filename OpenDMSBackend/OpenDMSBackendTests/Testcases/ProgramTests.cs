using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Utilities.InitializationStates;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core;
using OpenDMSBackend.Core.Configuration;
using System;
using System.Threading.Tasks;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace OpenDMSBackend.Tests.Testcases
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public void ProgramExitWithZeroAfterCallingStop()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                (bool, string) result = this.ProgramExitWithZeroAfterCallingStopAsync().WaitAndGetResult();
                GUtilities.AssertCondition(result.Item1, "Error-reason: " + result.Item2);
            }
        }

        private async Task<(bool, string)> ProgramExitWithZeroAfterCallingStopAsync()
        {
            Program host = new Program();
            try
            {
                Task<int> exitCodeTask = host.MainImplementationAsync(new string[] { });
                GUtilities.AssertCondition(GUtilities.RunWithTimeout(() => GUtilities.WaitUntilConditionIsTrue(() =>
                    {
                        try
                        {
                            IInitializationService<CommandlineParameter>? initService = host._InitializationService;
                            if (initService != null)
                            {
                                GUtilities.AssertCondition(initService.GetInitializationState() is not InitializationFailed);
                                return initService.GetInitializationState() is Initialized;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }
                    }), TimeSpan.FromSeconds(30)), "Service was not initialized in the given timespan.");
                host.Stop();
                int exitcode = await exitCodeTask;
                Assert.AreEqual(0, exitcode);
                return (true, "Success");
            }
            catch (Exception e)
            {
                return (false, GUtilities.GetExceptionMessage(e, "Error while running program") + "; Last log-lines: {" + string.Join(Environment.NewLine, host._Log.LastLogEntries.GetEntries()) + "}");
            }
        }
    }
}
