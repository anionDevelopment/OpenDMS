using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Services;

namespace OpenDMSBackend.Tests.Testcases.Services.PersistenceTests
{
    [TestClass]
    public class PersistenceTransientTests : PersistenceTestsBase
    {
        public override IPersistence GetPersistence()
        {
            return TestUtilities.Utilities.GetTransientPersistence();
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
