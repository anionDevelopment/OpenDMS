using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Tests.TestUtilities;

namespace OpenDMSBackend.Tests.Testcases.Services.DatabaseTests
{
    [TestClass]
    public class DatabaseTestsForPostgresSQL : DatabaseTestsBase
    {

        private DatabaseTestFrameworkForPostgreSQL? _DatabaseFramework;

        [TestInitialize]
        public void TestInitialize()
        {
            _DatabaseFramework = OpenDMSBackend.Tests.TestUtilities.Utilities.GetDatabaseTestFrameworkForPostgreSQL();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _DatabaseFramework?.Dispose();
            _DatabaseFramework = null;
        }

        protected override DatabaseTestFrameworkTemplate GetDatabaseTestFramework()
        {
            return _DatabaseFramework!;
        }



        [TestMethod(DisplayName = nameof(Migration000001Test))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void Migration000001Test()
        {
            this.Migration000001();
        }

        [TestMethod(DisplayName = nameof(AllMigrationsAreWorkingTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void AllMigrationsAreWorkingTest()
        {
            this.AllMigrationsAreWorking();
        }

        [TestMethod(DisplayName = nameof(LoadDocumentTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public override void LoadDocumentTest()
        {
            this.LoadDocument();
        }
    }
}
