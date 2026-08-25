using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Tests.Testcases.Services.PersistenceTests
{
    [TestClass]
    public class PersistenceTestsForTransient : PersistenceTestsBase
    {
        internal override PersistenceDisposable GetPersistence(ITimeService timeService)
        {
            (TransientPersistence, ISet<IDisposable>) result = TestUtilities.Utilities.GetTransientPersistence(timeService);
            return new PersistenceDisposable(result.Item1, result.Item2);
        }

        public override void Dispose()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }

        [TestMethod(DisplayName = nameof(DatabasePersistenceCreateDocumentTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void DatabasePersistenceCreateDocumentTest()
        {
            this.DatabasePersistenceCreateDocument();
        }

        [TestMethod(DisplayName = nameof(GetAmountOfDocumentsTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void GetAmountOfDocumentsTest()
        {
            this.GetAmountOfDocuments();
        }

        [TestMethod(DisplayName = nameof(PersistDocumentTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void PersistDocumentTest()
        {
            this.PersistDocument();
        }

        [TestMethod(DisplayName = nameof(AddStorageLocationTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void AddStorageLocationTest()
        {
            this.AddStorageLocation();
        }

        [TestMethod(DisplayName = nameof(AddFolderTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void AddFolderTest()
        {
            this.AddFolder();
        }

        [TestMethod(DisplayName = nameof(RenameStorageLocationTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void RenameStorageLocationTest()
        {
            this.RenameStorageLocation();
        }

        [TestMethod(DisplayName = nameof(RenameFolderTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void RenameFolderTest()
        {
            this.RenameFolder();
        }

        [TestMethod(DisplayName = nameof(CreateTagTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void CreateTagTest()
        {
            this.CreateTag();
        }

        [TestMethod(DisplayName = nameof(GetAllDocumentIdsTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void GetAllDocumentIdsTest()
        {
            this.GetAllDocumentIds();
        }

        [TestMethod(DisplayName = nameof(HardDeleteDocumentTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public override void HardDeleteDocumentTest()
        {
            this.HardDeleteDocument();
        }
    }
}
