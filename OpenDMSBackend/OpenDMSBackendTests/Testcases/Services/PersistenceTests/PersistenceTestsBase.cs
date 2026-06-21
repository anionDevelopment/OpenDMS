using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using OpenDMSBackend.Tests.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDMSBackend.Tests.Testcases.Services.PersistenceTests
{
    public abstract class PersistenceTestsBase : IDisposable
    {
        public PersistenceTestsBase()
        {
        }

        internal abstract PersistenceDisposable GetPersistence(ITimeService timeService);

        public abstract void Dispose();
        public abstract void PersistDocumentTest();
        public void PersistDocument()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 01, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0), new HashSet<string>(), "added-by-user-id");
                Assert.IsFalse(persistenceD.Persistence.IsDocument(testDocument.Id));

                //act
                persistenceD.Persistence.CreateDocument(testDocument);

                //assert
                Assert.IsTrue(persistenceD.Persistence.IsDocument(testDocument.Id));
            }
        }

        public abstract void DatabasePersistenceCreateDocumentTest();
        public void DatabasePersistenceCreateDocument()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange          
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);

                Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 02, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0), new HashSet<string>(), "added-by-user-id");

                //act
                persistenceD.Persistence.CreateDocument(testDocument);

                // assert
                Document reloadedDocument = persistenceD.Persistence.GetDocument(testDocument.Id);
                Assert.AreEqual(testDocument.OriginalFilename.Value, reloadedDocument.OriginalFilename.Value);
            }
        }

        public abstract void GetAmountOfDocumentsTest();
        public void GetAmountOfDocuments()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                /*
                Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 03, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0), new HashSet<string>(), "added-by-user-id");
                */
                /*
                uint expectedAmount1 = 0;
                uint expectedAmount2 = 1;

                //act
                uint actualAmount1 = persistenceD.Persistence.GetAmountOfDocuments();
                persistenceD.Persistence.CreateDocument(testDocument);
                uint actualAmount2 = persistenceD.Persistence.GetAmountOfDocuments();

                // assert

                Assert.AreEqual(expectedAmount1, actualAmount1);
                Assert.AreEqual(expectedAmount2, actualAmount2);
                */
            }
        }

        public abstract void AddStorageLocationTest();
        public void AddStorageLocation()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                string name = $"sl-{Guid.NewGuid()}";

                //act
                string storageLocationId = persistenceD.Persistence.AddStoragLocation(name);

                //assert
                Assert.IsTrue(persistenceD.Persistence.IsStorageLocation(storageLocationId));
                StorageLocation reloaded = persistenceD.Persistence.GetStorageLocation(storageLocationId);
                Assert.AreEqual(name, reloaded.Name);
            }
        }

        public abstract void AddFolderTest();
        public void AddFolder()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                string name = $"folder-{Guid.NewGuid()}";

                //act
                string folderId = persistenceD.Persistence.AddFolder(name);

                //assert
                Assert.IsTrue(persistenceD.Persistence.IsFolder(folderId));
                Folder reloaded = persistenceD.Persistence.GetFolder(folderId);
                Assert.AreEqual(name, reloaded.Name);
            }
        }

        public abstract void RenameStorageLocationTest();
        public void RenameStorageLocation()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                string originalName = $"sl-{Guid.NewGuid()}";
                string newName = $"sl-renamed-{Guid.NewGuid()}";
                string storageLocationId = persistenceD.Persistence.AddStoragLocation(originalName);

                //act
                persistenceD.Persistence.Rename(storageLocationId, newName);

                //assert
                StorageLocation reloaded = persistenceD.Persistence.GetStorageLocation(storageLocationId);
                Assert.AreEqual(newName, reloaded.Name);
            }
        }

        public abstract void RenameFolderTest();
        public void RenameFolder()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                string originalName = $"folder-{Guid.NewGuid()}";
                string newName = $"folder-renamed-{Guid.NewGuid()}";
                string folderId = persistenceD.Persistence.AddFolder(originalName);

                //act
                persistenceD.Persistence.Rename(folderId, newName);

                //assert
                Folder reloaded = persistenceD.Persistence.GetFolder(folderId);
                Assert.AreEqual(newName, reloaded.Name);
            }
        }

        public abstract void CreateTagTest();
        public void CreateTag()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                string tagId = Guid.NewGuid().ToString();
                string tagName = $"tag-{Guid.NewGuid()}";
                Tag tag = new Tag(tagId, tagName, new ExtendedColor(0xFF0000));

                //act
                persistenceD.Persistence.CreateTag(tag);

                //assert
                TagDTO[] allTags = persistenceD.Persistence.GetAllTags();
                Assert.IsTrue(allTags.Any(t => t.Id == tagId && t.Name == tagName));
            }
        }

        public abstract void GetAllDocumentIdsTest();
        public void GetAllDocumentIds()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 04, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0), new HashSet<string>(), "added-by-user-id");

                //act
                persistenceD.Persistence.CreateDocument(testDocument);
                ISet<string> allDocumentIds = persistenceD.Persistence.GetAllDocumentIds();

                //assert
                Assert.IsTrue(allDocumentIds.Contains(testDocument.Id));
            }
        }

    }
}
