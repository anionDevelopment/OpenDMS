using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenDMSBackend.Tests.Testcases.Services.DatabaseTests
{
    [TestClass]
    public class DatabaseTestsForPostgresSQL : DatabaseTestsBase
    {
        protected override DatabaseTestFrameworkTemplate GetDatabaseTestFrameworkImplementation()
        {
            return OpenDMSBackend.Tests.TestUtilities.Utilities.GetDatabaseTestFrameworkForPostgreSQL();
        }


        [TestMethod(DisplayName = nameof(Migration000001Test))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public override void Migration000001Test()
        {
            this.Migration000001();
        }

        [TestMethod(DisplayName = nameof(AllMigrationsAreWorkingTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public override void AllMigrationsAreWorkingTest()
        {
            this.AllMigrationsAreWorking();
        }

        [TestMethod(DisplayName = nameof(LoadDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public override void LoadDocumentTest()
        {
            this.LoadDocument();
        }
    }
}
