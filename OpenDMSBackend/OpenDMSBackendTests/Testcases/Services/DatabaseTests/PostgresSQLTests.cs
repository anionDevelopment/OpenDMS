using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities;

namespace OpenDMSBackend.Tests.Testcases.Services.DatabaseTests
{
    [TestClass]
    public class PostgresSQLTests : DatabaseTestsBase
    {
        public PostgresSQLTests() : base(new DatabaseInteractorPostgreSQL(new DatabasePersistenceConfiguration() {
            DatabaseType = "PostgreSQL",
            DatabaseConnectionString = OpenDMSBackend.Tests.TestUtilities.Utilities.GetTestPostgreSQLConnectionString()
        }))
        {
        }

        protected override DatabaseTestFrameworkTemplate GetDatabaseTestFramework()
        {
            return new DatabaseTestFrameworkForMariaDB(this.DatabaseManager.Log);
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
