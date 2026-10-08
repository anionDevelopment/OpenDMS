using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Utilities.InitializationStates;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using OpenDMSBackend.Core.Services;
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
                Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 01, TimeSpan.Zero),1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), "added-by-user-id");
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

                Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 02, TimeSpan.Zero),1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), "added-by-user-id");

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
                Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 03, TimeSpan.Zero),1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), "added-by-user-id");
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
                Tag tag = new Tag(tagId, tagName, new ExtendedColor(0xFF0000), null);

                //act
                persistenceD.Persistence.CreateTag(tag);

                //assert
                IEnumerable<Tag> allTags = persistenceD.Persistence.GetAllTags();
                Assert.IsTrue(allTags.Any(t => t.Id == tagId && t.Name == tagName));
            }
        }

        public abstract void UpdateTagTest();
        public void UpdateTag()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                Tag tag = new Tag(Guid.NewGuid().ToString(), $"tag-{Guid.NewGuid()}", new ExtendedColor(0xFF0000), null);
                persistenceD.Persistence.CreateTag(tag);
                string newName = $"tag-{Guid.NewGuid()}";

                //act
                tag.Name = newName;
                tag.Color = new ExtendedColor(0x00FF00);
                persistenceD.Persistence.UpdateTag(tag);

                //assert
                Tag reloaded = persistenceD.Persistence.GetTag(tag.Id);
                Assert.AreEqual(newName, reloaded.Name);
                Assert.AreEqual(new ExtendedColor(0x00FF00).ColorCode, reloaded.Color.ColorCode);
            }
        }

        public abstract void DeleteTagTest();
        public void DeleteTag()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                Tag tag = new Tag(Guid.NewGuid().ToString(), $"tag-{Guid.NewGuid()}", new ExtendedColor(0xFF0000), null);
                persistenceD.Persistence.CreateTag(tag);
                Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 05, TimeSpan.Zero), 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, "some ocr content", new byte[] { 9, 8 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), "added-by-user-id");
                persistenceD.Persistence.CreateDocument(testDocument);
                persistenceD.Persistence.AssignTag(testDocument.Id, tag.Id);

                //act
                persistenceD.Persistence.DeleteTag(tag.Id);

                //assert: the tag is gone and is not assigned to any document anymore, because an assignment of a no longer existing tag would be a dangling reference.
                Assert.IsFalse(persistenceD.Persistence.GetAllTags().Any(existingTag => existingTag.Id == tag.Id));
                Assert.AreEqual(0, persistenceD.Persistence.GetTagIdsOfDocument(testDocument.Id).Count);
            }
        }

        public abstract void UpdateMetadataFieldDefinitionTest();
        public void UpdateMetadataFieldDefinition()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                string storageLocationId = persistenceD.Persistence.AddStoragLocation($"storageLocation-{Guid.NewGuid()}");
                MetadataFieldDefinition definition = new MetadataFieldDefinition(Guid.NewGuid().ToString(), storageLocationId, "contact", MetadataFieldType.String);
                persistenceD.Persistence.CreateMetadataFieldDefinition(definition);

                //act
                definition.Name = "sender";
                persistenceD.Persistence.UpdateMetadataFieldDefinition(definition);

                //assert
                MetadataFieldDefinition reloaded = persistenceD.Persistence.GetMetadataFieldDefinition(definition.Id);
                Assert.AreEqual("sender", reloaded.Name);
                Assert.AreEqual(MetadataFieldType.String, reloaded.Type);
            }
        }

        public abstract void HardDeleteDocumentTest();
        public void HardDeleteDocument()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                Tag tag = new Tag(Guid.NewGuid().ToString(), $"tag-{Guid.NewGuid()}", new ExtendedColor(0xFF0000), null);
                persistenceD.Persistence.CreateTag(tag);
                Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 05, TimeSpan.Zero), 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, "some ocr content", new byte[] { 9, 8 }, false, default, new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero), CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), "added-by-user-id")
                {
                    AISummaryShort = "short summary",
                    AISummaryLong = "long summary"
                };
                persistenceD.Persistence.CreateDocument(testDocument);
                persistenceD.Persistence.AssignTag(testDocument.Id, tag.Id);
                //the retention-deadline is in the past, so the document is initially eligible for hard-deletion.
                Assert.IsTrue(persistenceD.Persistence.GetIdsOfDocumentsWhichMustBeHardDeletedNow().Contains(testDocument.Id));

                //act
                persistenceD.Persistence.HardDelete(testDocument.Id);

                //assert: the row is kept for traceability but the content is stripped and the document is marked as hard-deleted.
                Assert.IsTrue(persistenceD.Persistence.IsDocument(testDocument.Id));
                Document reloaded = persistenceD.Persistence.GetDocument(testDocument.Id);
                Assert.IsTrue(reloaded.IsHardDeleted);
                Assert.AreEqual(0, reloaded.Content.Length);
                Assert.AreEqual(0, reloaded.Preview.Length);
                Assert.AreEqual(string.Empty, reloaded.OCRContent);
                Assert.IsNull(reloaded.AISummaryShort);
                Assert.IsNull(reloaded.AISummaryLong);
                Assert.AreEqual(0, reloaded.Tags.Count);
                //a hard-deleted document is no longer eligible for hard-deletion so that the housekeeping does not run endlessly.
                Assert.IsFalse(persistenceD.Persistence.GetIdsOfDocumentsWhichMustBeHardDeletedNow().Contains(testDocument.Id));
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
                Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 04, TimeSpan.Zero),1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), "added-by-user-id");

                //act
                persistenceD.Persistence.CreateDocument(testDocument);
                ISet<string> allDocumentIds = persistenceD.Persistence.GetAllDocumentIds();

                //assert
                Assert.IsTrue(allDocumentIds.Contains(testDocument.Id));
            }
        }

        public abstract void UserCanNotDoAdministratorOnlyActionTest();
        public void UserCanNotDoAdministratorOnlyAction()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                IBusinessLogicService businessLogicService = InitializeBusinessLogic(persistenceD.Persistence, timeService);
                string userId = businessLogicService.Register($"user-{Guid.NewGuid()}", "password");
                Assert.IsFalse(businessLogicService.UserIsAdministrator(userId));

                //act & assert: a user who is not an administrator must not be able to grant themselves the administrator-role.
                Assert.ThrowsExactly<NotAuthorizedException>(() => businessLogicService.SetRolesOfUser(userId, userId, new HashSet<string>() { CodeUnitSpecificConstants.RolenameAdmins }));
                Assert.IsFalse(businessLogicService.UserIsAdministrator(userId), "The rejected role-change must not have been applied.");
            }
        }

        public abstract void UserCanNotReadDocumentOfAnotherUserWithoutPermissionTest();
        public void UserCanNotReadDocumentOfAnotherUserWithoutPermission()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                //arrange
                TimeService timeService = new TimeService();
                using PersistenceDisposable persistenceD = this.GetPersistence(timeService);
                IBusinessLogicService businessLogicService = InitializeBusinessLogic(persistenceD.Persistence, timeService);
                string ownerUserId = businessLogicService.Register($"owner-{Guid.NewGuid()}", "password");
                string otherUserId = businessLogicService.Register($"other-{Guid.NewGuid()}", "password");
                string storageLocationId = persistenceD.Persistence.AddStoragLocation($"sl-{Guid.NewGuid()}");
                persistenceD.Persistence.SetOwnerOfStorageLocation(storageLocationId, ownerUserId);
                Document document = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 06, TimeSpan.Zero), 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), ownerUserId);
                persistenceD.Persistence.CreateDocument(document);
                persistenceD.Persistence.SetParentOfContainee(document, storageLocationId);
                //the owner can read the document, so a refusal for the other user is caused by the missing permission and not by a broken setup.
                Assert.AreEqual(document.Id, businessLogicService.GetDocument(ownerUserId, document.Id).Id);

                //act & assert
                Assert.ThrowsExactly<NotAuthorizedException>(() => businessLogicService.GetDocument(otherUserId, document.Id));
                Assert.ThrowsExactly<NotAuthorizedException>(() => businessLogicService.GetDocumentPreview(otherUserId, document.Id));
            }
        }

        /// <summary>Builds the business-logic on top of the given persistence and initializes it (roles and the administrator), so that permission-checks can be verified for every persistence.</summary>
        private static IBusinessLogicService InitializeBusinessLogic(IPersistence persistence, ITimeService timeService)
        {
            ServicesForTests.CreateServices(persistence, timeService, true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService);
            initializationService.Initialize(new CommandlineParameter());
            Assert.IsInstanceOfType<Initialized>(initializationService.GetInitializationState());
            return businessLogicService;
        }
    }
}
