using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Tests.TestUtilities;

namespace OpenDMSBackend.Tests.Testcases.Services
{
    [TestClass]
    public class DatabaseMariaDBTests : DatabaseTestsBase
    {
        protected override DatabaseTestFrameworkTemplate GetDatabaseTestFramework()
        {
            return new DatabaseTestFrameworkForMariaDB();
        }

        [TestMethod(nameof(Migration000001Test))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public override void Migration000001Test()
        {
            base.Migration000001();
        }

        [TestMethod(nameof(GenerateDatabaseGenerationScriptTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public override void GenerateDatabaseGenerationScriptTest()
        {
            base.GenerateDatabaseGenerationScript();
        }
    }
}
