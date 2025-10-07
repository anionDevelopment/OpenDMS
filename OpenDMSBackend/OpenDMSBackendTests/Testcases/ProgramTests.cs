using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                var result = ProgramExitWithZeroAfterCallingStopAsync().WaitAndGetResult();
                Assert.IsTrue(result.Item1, result.Item2);
            }
        }

        private async Task<(bool, string)> ProgramExitWithZeroAfterCallingStopAsync()
        {
            try
            {
                Program host = new Program();
                Task<int> exitCodeTask = host.MainImplementationAsync(new string[] { });
                await Task.Delay(TimeSpan.FromSeconds(5));
                host._HostApplicationLifetime!.StopApplication();
                var exitcode = await exitCodeTask;
                Assert.AreEqual(0, exitcode);
                return (true, "Success");
            }
            catch (Exception e)
            {
                return (false, GUtilities.GetExceptionMessage(e, "Error while running program"));
            }
        }
    }
}
