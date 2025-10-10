using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenDMSBackend.Tests.Testcases.Services.PersistenceTests
{
    [TestClass]
    public class MariaDBPersistenceTests : PersistenceDatabaseTestsBase
    {
        protected override DatabaseTestFrameworkTemplate GetDatabaseTestFramework()
        {
            return OpenDMSBackend.Tests.TestUtilities.Utilities.GetDatabaseTestFrameworkForMariaDB();
        }

        [TestMethod(nameof(DatabasePersistenceCreateDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void DatabasePersistenceCreateDocumentTest()
        {
            this.DatabasePersistenceCreateDocument();
        }


        [TestMethod(nameof(GetAmountOfDocumentsTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void GetAmountOfDocumentsTest()
        {
            this.GetAmountOfDocuments();
        }

        [TestMethod(nameof(PersistDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void PersistDocumentTest()
        {
            this.PersistDocument();
        }

    }
}
