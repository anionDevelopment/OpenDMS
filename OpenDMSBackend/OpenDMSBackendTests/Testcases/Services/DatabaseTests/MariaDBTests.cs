using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Database;
using OpenDMSBackend.Tests.TestUtilities;

namespace OpenDMSBackend.Tests.Testcases.Services.DatabaseTests
{
    [TestClass]
    public class MariaDBTests : DatabaseTestsBase
    {
        protected override DatabaseTestFrameworkTemplate GetDatabaseTestFramework()
        {
            return new DatabaseTestFrameworkForMariaDB();
        }

        protected override IDatabaseManager GetDatabaseManager()
        {
            return new DatabaseManagerMariaDB();
        }

        [TestMethod(nameof(Migration000001Test))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public override void Migration000001Test()
        {
            Migration000001();
        }

        [TestMethod(nameof(GenerateDatabaseGenerationScriptTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public override void GenerateDatabaseGenerationScriptTest()
        {
            GenerateDatabaseGenerationScript();
        }
    }
}
