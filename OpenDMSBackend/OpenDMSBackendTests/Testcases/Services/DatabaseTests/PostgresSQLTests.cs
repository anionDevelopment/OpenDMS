using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Database;
using OpenDMSBackend.Tests.TestUtilities;

namespace OpenDMSBackend.Tests.Testcases.Services.DatabaseTests
{
    [TestClass]
    public class PostgresSQLTests : DatabaseTestsBase
    {
        protected override DatabaseTestFrameworkTemplate GetDatabaseTestFramework()
        {
            return new DatabaseTestFrameworkForPostgreSQL();
        }

        protected override IDatabaseManager GetDatabaseManager()
        {
            return new DatabaseManagerPostgreSQL();
        }

        [TestMethod(nameof(Migration000001Test))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public override void Migration000001Test()
        {
            lock (DatabaseTestsLockObject)
            {
                this.Migration000001IsWorking();
            }
        }

        [TestMethod(nameof(GenerateDatabaseGenerationScriptTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public override void GenerateDatabaseGenerationScriptTest()
        {
            lock (DatabaseTestsLockObject)
            {
                this.GenerateDatabaseGenerationScript();
            }
        }
    }
}
