using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Database;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities;

namespace OpenDMSBackend.Tests.Testcases.Services
{
    [TestClass]
    public class PersistenceTransientTests : PersistenceTestsBase
    {
        public override IPersistence GetPersistence()
        {
            return OpenDMSBackend.Tests.TestUtilities.Utilities.GetTransientPersistence();
        }



        [TestMethod(nameof(PersistDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void DatabasePersistenceCreateDocumentTest()
        {
            DatabasePersistenceCreateDocument();
        }


        [TestMethod(nameof(PersistDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void GetAmountOfDocumentsTest()
        {
            GetAmountOfDocuments();
        }

        [TestMethod(nameof(PersistDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void PersistDocumentTest()
        {
            PersistDocument();
        }
    }
}
