using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Tests.TestUtilities;

namespace OpenDMSBackend.Tests.Testcases.Services.PersistenceTests
{
    [TestClass]
    public class PersistenceTestsForMariaDB : PersistenceTestsBaseForDatabase
    {
        private DatabaseTestFrameworkForMariaDB? _DatabaseFramework;

        [TestInitialize]
        public void TestInitialize()
        {
            _DatabaseFramework = OpenDMSBackend.Tests.TestUtilities.Utilities.GetDatabaseTestFrameworkForMariaDB();
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


        [TestMethod(DisplayName = nameof(DatabasePersistenceCreateDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void DatabasePersistenceCreateDocumentTest()
        {
            this.DatabasePersistenceCreateDocument();
        }

        [TestMethod(DisplayName = nameof(GetAmountOfDocumentsTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void GetAmountOfDocumentsTest()
        {
            this.GetAmountOfDocuments();
        }

        [TestMethod(DisplayName = nameof(PersistDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void PersistDocumentTest()
        {
            this.PersistDocument();
        }

        [TestMethod(DisplayName = nameof(AddStorageLocationTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void AddStorageLocationTest()
        {
            this.AddStorageLocation();
        }

        [TestMethod(DisplayName = nameof(AddFolderTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void AddFolderTest()
        {
            this.AddFolder();
        }

        [TestMethod(DisplayName = nameof(RenameStorageLocationTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void RenameStorageLocationTest()
        {
            this.RenameStorageLocation();
        }

        [TestMethod(DisplayName = nameof(RenameFolderTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void RenameFolderTest()
        {
            this.RenameFolder();
        }

        [TestMethod(DisplayName = nameof(CreateTagTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void CreateTagTest()
        {
            this.CreateTag();
        }

        [TestMethod(DisplayName = nameof(GetAllDocumentIdsTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void GetAllDocumentIdsTest()
        {
            this.GetAllDocumentIds();
        }

        [TestMethod(DisplayName = nameof(HardDeleteDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void HardDeleteDocumentTest()
        {
            this.HardDeleteDocument();
        }

    }
}
