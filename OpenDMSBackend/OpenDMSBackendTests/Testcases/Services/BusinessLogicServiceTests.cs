using GRYLibrary.Core.APIServer;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Res;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDMSBackend.Tests.Testcases.Services
{
    [TestClass]
    public class BusinessLogicServiceTests
    {
        private void InitializeServices(bool registrationIsEnabled, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence)
        {
            ITimeService timeService = new TimeService();
            IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> persistedAPIServerConfiguration = new PersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>();
            persistedAPIServerConfiguration.ApplicationSpecificConfiguration = new CodeUnitSpecificConfiguration();
            persistedAPIServerConfiguration.ApplicationSpecificConfiguration.RegistrationIsEnabled = registrationIsEnabled;
            ApplicationConstants<CodeUnitSpecificConstants> constants = new ApplicationConstants<CodeUnitSpecificConstants>(GeneralConstants.CodeUnitName, GeneralConstants.CodeUnitVersion, Version3.Parse(GeneralConstants.CodeUnitVersion), TestRun.Instance, OpenDMSBackend.Core.Misc.Utilities.GetEnvironmentTargetType(), new CodeUnitSpecificConstants());
            constants.BaseFolder = APIServer<CodeUnitSpecificConstants, PersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>, CommandlineParameter>.GetDefaultBaseFolder(constants, true);
            IServerLog logger = new ServerLog(new GRYLogConfiguration(true), constants.GetLogFolder());
            IAuditLog auditLog = new AuditLog(new GRYLogConfiguration(true), constants.GetLogFolder());
            IIdGenerator<ulong> idGenerator = new OpenDMSBackend.Core.Services.IdGenerator();
            (TransientPersistence, ISet<IDisposable>) databasePersistenceD = OpenDMSBackend.Tests.TestUtilities.Utilities.GetTransientPersistence(timeService);
            persistence = databasePersistenceD.Item1;
            IAuthenticationService<User> authenticationService = new PersistentAuthenticationService(timeService, persistence, logger, constants);
            IGeneralResourceLoader generalResourceLoader = new OpenDMSBackend.Core.Services.GeneralResourceLoader();
            Mock<IOCRServiceClient> ocrServiceClientMock = new Mock<IOCRServiceClient>(MockBehavior.Strict);
            businessLogicService = new BusinessLogicService(persistence, authenticationService, timeService, constants, logger, persistedAPIServerConfiguration, ocrServiceClientMock.Object, new OpenDMSBackend.Core.Services.AISummaryServiceClientMock(), idGenerator, generalResourceLoader, auditLog);
            IExampleDataCreator exampleDataCreator = new ExampleDataCreator(businessLogicService, authenticationService);
            initializationService = new InitializationService(authenticationService, businessLogicService, logger, constants, exampleDataCreator, persistence, idGenerator);
        }

        [TestMethod(DisplayName = nameof(DatabaseInitializationTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void DatabaseInitializationTest()
        {
            // arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence _);
            string adminUserName = CodeUnitSpecificConstants.UsernameAdmin;

            // act
            initializationService.Initialize(new CommandlineParameter());

            // assert
            Assert.IsTrue(businessLogicService.UserWithNameExists(adminUserName));
            // TODO add more assertions
        }

        [TestMethod(DisplayName = nameof(RegisterTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void RegisterTest()
        {
            // arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            string user = "someuser";
            string password = "somepassword";
            Assert.IsFalse(persistence.UserWithNameExists(user));

            // act
            string userId = businessLogicService.Register(user, password);

            // assert
            Assert.IsTrue(persistence.UserWithIdExists(userId));
            Assert.IsTrue(businessLogicService.UserWithNameExists(user));
            // TODO add more assertions
        }

        /// <remarks>
        /// A duplicate name must be refused by the business-logic, not only by the unique-constraint of the
        /// database: the constraint does not exist in the transient persistence and its violation would surface
        /// as an internal error instead of a usable one.
        /// </remarks>
        [TestMethod(DisplayName = nameof(RegisterWithAlreadyTakenUsernameIsRejectedTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void RegisterWithAlreadyTakenUsernameIsRejectedTest()
        {
            // arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            string user = "someuser";
            string userId = businessLogicService.Register(user, "somepassword");

            // act & assert
            Assert.ThrowsExactly<BadRequestException>(() => businessLogicService.Register(user, "anotherpassword"));

            // assert: the existing account is untouched and no second one was created
            Assert.IsTrue(persistence.UserWithIdExists(userId));
        }

        [TestMethod(DisplayName = nameof(GetLatestDocumentsTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetLatestDocumentsTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            string user1Id = "user1Id";
            persistence.AddUser(new User() { Id = user1Id, });
            string user2Id = "user2Id";
            persistence.AddUser(new User() { Id = user2Id, });

            string storageLocation1Id = persistence.AddStoragLocation("storageLocation1");
            string storageLocation2Id = persistence.AddStoragLocation("storageLocation2");
            persistence.SetOwnerOfStorageLocation(storageLocation1Id, user1Id);
            persistence.SetOwnerOfStorageLocation(storageLocation2Id, user2Id);

            Document testDocument1 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title1"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 01, 00, TimeSpan.Zero), 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), user1Id);
            Document testDocument2 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title2"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 02, 00, TimeSpan.Zero),2, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), user1Id);
            Document testDocument3 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title3"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 03, 00, TimeSpan.Zero),3, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), user1Id);
            Document testDocument4 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title4"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 04, 00, TimeSpan.Zero),4, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), user1Id);
            Document testDocument5 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title5"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 05, 00, TimeSpan.Zero),5, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), user1Id);
            Document testDocument6 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title6"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 06, 00, TimeSpan.Zero),6, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), user2Id);
            Document testDocument7 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title7"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 07, 00, TimeSpan.Zero),7, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), user1Id);
            List<Document> expectedDocuments=new List<Document> {  testDocument2, testDocument3, testDocument4, testDocument5, testDocument7  };
            List<string> expectedIds = expectedDocuments.Select(d => d.Id).ToList();

            persistence.CreateDocument(testDocument1);
            persistence.SetParentOfContainee(testDocument1, storageLocation1Id);
            persistence.CreateDocument(testDocument2);
            persistence.SetParentOfContainee(testDocument2, storageLocation1Id);
            persistence.CreateDocument(testDocument3);
            persistence.SetParentOfContainee(testDocument3, storageLocation1Id);
            persistence.CreateDocument(testDocument4);
            persistence.SetParentOfContainee(testDocument4, storageLocation1Id);
            persistence.CreateDocument(testDocument5);
            persistence.SetParentOfContainee(testDocument5, storageLocation1Id);
            persistence.CreateDocument(testDocument6);
            persistence.SetParentOfContainee(testDocument6, storageLocation2Id);
            persistence.CreateDocument(testDocument7);
            persistence.SetParentOfContainee(testDocument7, storageLocation1Id);

            //act
            IList<DocumentPreview> actualDocuments = businessLogicService.GetLatestDocuments(user1Id).OrderBy(d=>d.ReadableId).ToList();

            // assert
            List<string> actualIds = actualDocuments.Select(prev => prev.Id).ToList();
            Assert.IsTrue(actualIds.ToHashSet().SetEquals(expectedIds));
        }

        [TestMethod(DisplayName = nameof(SoftDeleteDocumentTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void SoftDeleteDocumentTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            string userId = "user1Id";
            persistence.AddUser(new User() { Id = userId, });
            string storageLocationId = persistence.AddStoragLocation("storageLocation1");
            persistence.SetOwnerOfStorageLocation(storageLocationId, userId);
            Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 01, 00, TimeSpan.Zero), 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), userId);
            persistence.CreateDocument(testDocument);
            persistence.SetParentOfContainee(testDocument, storageLocationId);
            Assert.IsFalse(persistence.GetDocument(testDocument.Id).IsSoftDeleted);

            //act
            businessLogicService.SoftDelete(userId, testDocument.Id, "obsolete");

            //assert
            Assert.IsTrue(persistence.GetDocument(testDocument.Id).IsSoftDeleted, "The document should be marked as soft-deleted.");
            Assert.IsTrue(persistence.IsDocument(testDocument.Id), "A soft-deleted document must not be removed physically.");
        }

        [TestMethod(DisplayName = nameof(SoftDeleteStorageLocationSoftDeletesContainedDocumentsTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void SoftDeleteStorageLocationSoftDeletesContainedDocumentsTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            string userId = "user1Id";
            persistence.AddUser(new User() { Id = userId, });
            string storageLocationId = persistence.AddStoragLocation("storageLocation1");
            persistence.SetOwnerOfStorageLocation(storageLocationId, userId);
            Document testDocument1 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title1"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 01, 00, TimeSpan.Zero), 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), userId);
            Document testDocument2 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title2"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 02, 00, TimeSpan.Zero),2, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), userId);
            persistence.CreateDocument(testDocument1);
            persistence.SetParentOfContainee(testDocument1, storageLocationId);
            persistence.CreateDocument(testDocument2);
            persistence.SetParentOfContainee(testDocument2, storageLocationId);

            //act
            businessLogicService.SoftDelete(userId, storageLocationId, "obsolete");

            //assert
            Assert.IsTrue(persistence.GetDocument(testDocument1.Id).IsSoftDeleted);
            Assert.IsTrue(persistence.GetDocument(testDocument2.Id).IsSoftDeleted);
        }

        [TestMethod(DisplayName = nameof(GenerateAISummaryTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GenerateAISummaryTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            string userId = "user1Id";
            persistence.AddUser(new User() { Id = userId, });
            string storageLocationId = persistence.AddStoragLocation("storageLocation1");
            persistence.SetOwnerOfStorageLocation(storageLocationId, userId);
            Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 01, 00, TimeSpan.Zero), 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, "some ocr content", new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), userId);
            persistence.CreateDocument(testDocument);
            persistence.SetParentOfContainee(testDocument, storageLocationId);
            Assert.IsNull(persistence.GetDocument(testDocument.Id).AISummaryShort);

            //act
            businessLogicService.GenerateAISummary(userId, testDocument.Id);

            //assert
            Document reloaded = persistence.GetDocument(testDocument.Id);
            Assert.IsFalse(string.IsNullOrEmpty(reloaded.AISummaryShort), "The short AI-summary should be generated.");
            Assert.IsFalse(string.IsNullOrEmpty(reloaded.AISummaryLong), "The long AI-summary should be generated.");
        }

        [TestMethod(DisplayName = nameof(AutoGenerateAISummarySettingTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void AutoGenerateAISummarySettingTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());

            //act & assert
            Assert.IsFalse(businessLogicService.GetAutoGenerateAISummary(), "The setting must default to false.");
            persistence.SetSetting(CodeUnitSpecificConstants.SettingKeyAutoGenerateAISummary, true.ToString());
            Assert.IsTrue(businessLogicService.GetAutoGenerateAISummary(), "The setting must reflect the stored value.");
        }

        [TestMethod(DisplayName = nameof(SetAutoGenerateAISummaryRequiresAdminTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void SetAutoGenerateAISummaryRequiresAdminTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            string nonAdminUserId = "nonAdminUserId";
            persistence.AddUser(new User() { Id = nonAdminUserId, });

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.SetAutoGenerateAISummary(nonAdminUserId, true);
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A non-admin-user must not be allowed to change general settings.");
        }

        [TestMethod(DisplayName = nameof(VersionHistoryAndLatestVersionFilteringTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void VersionHistoryAndLatestVersionFilteringTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            string userId = "user1Id";
            persistence.AddUser(new User() { Id = userId, });
            string storageLocationId = persistence.AddStoragLocation("storageLocation1");
            persistence.SetOwnerOfStorageLocation(storageLocationId, userId);
            Document oldVersion = new Document(Guid.NewGuid().ToString(), OneLineString.From("title-v1"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 01, 00, TimeSpan.Zero), 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), userId);
            Document newVersion = new Document(Guid.NewGuid().ToString(), OneLineString.From("title-v2"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 02, 00, TimeSpan.Zero), 2, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), userId);
            persistence.CreateDocument(oldVersion);
            persistence.SetParentOfContainee(oldVersion, storageLocationId);
            persistence.CreateDocument(newVersion);
            persistence.SetParentOfContainee(newVersion, storageLocationId);
            string logicalDocumentId = Guid.NewGuid().ToString();
            persistence.AddDocumentVersion(new DocumentVersionEntry(logicalDocumentId, oldVersion.Id, 1, new DateTimeOffset(2025, 11, 17, 20, 01, 00, TimeSpan.Zero)));
            persistence.AddDocumentVersion(new DocumentVersionEntry(logicalDocumentId, newVersion.Id, 2, new DateTimeOffset(2025, 11, 17, 20, 02, 00, TimeSpan.Zero)));
            persistence.SetIsLatestVersion(oldVersion.Id, false);

            //act
            List<DocumentPreview> history = businessLogicService.GetVersionHistory(userId, newVersion.Id).ToList();
            List<string> latestIds = businessLogicService.GetLatestDocuments(userId).Select(document => document.Id).ToList();

            //assert
            Assert.AreEqual(2, history.Count, "The version-history must contain both versions.");
            Assert.AreEqual(oldVersion.Id, history[0].Id, "The history must start with the oldest version.");
            Assert.AreEqual(newVersion.Id, history[1].Id, "The history must end with the newest version.");
            Assert.IsFalse(latestIds.Contains(oldVersion.Id), "A superseded version must not appear in the latest-documents-list.");
            Assert.IsTrue(latestIds.Contains(newVersion.Id), "The newest version must appear in the latest-documents-list.");
        }

        [TestMethod(DisplayName = nameof(UploadNewVersionTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void UploadNewVersionTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            string userId = "user1Id";
            persistence.AddUser(new User() { Id = userId, });
            string storageLocationId = persistence.AddStoragLocation("storageLocation1");
            persistence.SetOwnerOfStorageLocation(storageLocationId, userId);
            Document oldVersion = new Document(Guid.NewGuid().ToString(), OneLineString.From("title-v1"), OneLineString.From("file.txt"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.txt"), new DateTimeOffset(2025, 11, 17, 20, 01, 00, TimeSpan.Zero), 1, new HashSet<Tag>(), OneLineString.From("text/plain"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), userId);
            persistence.CreateDocument(oldVersion);
            persistence.SetParentOfContainee(oldVersion, storageLocationId);
            persistence.AddDocumentVersion(new DocumentVersionEntry(Guid.NewGuid().ToString(), oldVersion.Id, 1, new DateTimeOffset(2025, 11, 17, 20, 01, 00, TimeSpan.Zero)));

            //act
            string newVersionId = businessLogicService.UploadNewVersion(userId, oldVersion.Id, "title-v2", "file.txt", new byte[] { 5, 6, 7 }, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>());

            //assert
            Assert.AreNotEqual(oldVersion.Id, newVersionId, "The new version must be a new document.");
            Assert.IsFalse(persistence.GetDocument(oldVersion.Id).IsLatestVersion, "The old version must no longer be the latest version.");
            Assert.IsTrue(persistence.GetDocument(newVersionId).IsLatestVersion, "The new version must be the latest version.");
            Assert.AreEqual(storageLocationId, persistence.GetParentIdOfContainee(newVersionId), "The new version must be stored in the same folder as the old version.");
            List<string> historyIds = businessLogicService.GetVersionHistory(userId, oldVersion.Id).Select(document => document.Id).ToList();
            CollectionAssert.Contains(historyIds, oldVersion.Id);
            CollectionAssert.Contains(historyIds, newVersionId);
        }

        /// <summary>Creates a user which is moderator of a new storage-location containing one document and returns the relevant ids.</summary>
        private static void SetupModeratorWithDocument(IPersistence persistence, out string userId, out string storageLocationId, out string documentId)
        {
            userId = "metadata-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = userId, });
            storageLocationId = persistence.AddStoragLocation("metadata-storageLocation");
            persistence.SetOwnerOfStorageLocation(storageLocationId, userId);
            Document document = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 01, 00, TimeSpan.Zero), 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), userId);
            persistence.CreateDocument(document);
            persistence.SetParentOfContainee(document, storageLocationId);
            documentId = document.Id;
        }

        [TestMethod(DisplayName = nameof(DefineAndGetMetadataFieldTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void DefineAndGetMetadataFieldTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string _);

            //act
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "tax-relevant", MetadataFieldType.Boolean);

            //assert
            List<MetadataFieldDefinition> fields = businessLogicService.GetMetadataFields(userId, storageLocationId).ToList();
            Assert.AreEqual(1, fields.Count);
            Assert.AreEqual(fieldId, fields[0].Id);
            Assert.AreEqual("tax-relevant", fields[0].Name);
            Assert.AreEqual(MetadataFieldType.Boolean, fields[0].Type);
            Assert.AreEqual(storageLocationId, fields[0].StorageLocationId);
        }

        [TestMethod(DisplayName = nameof(DefineMetadataFieldRequiresModeratorTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void DefineMetadataFieldRequiresModeratorTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string _, out string storageLocationId, out string _);
            string otherUserId = "other-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = otherUserId, });

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.DefineMetadataField(otherUserId, storageLocationId, "some-field", MetadataFieldType.String);
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A user which is not a moderator of the storage-location must not be allowed to define a metadata-field.");
        }

        [TestMethod(DisplayName = nameof(DefineDuplicateMetadataFieldNameTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void DefineDuplicateMetadataFieldNameTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string _);
            businessLogicService.DefineMetadataField(userId, storageLocationId, "deadline", MetadataFieldType.String);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.DefineMetadataField(userId, storageLocationId, "DEADLINE", MetadataFieldType.Boolean);
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Defining a second metadata-field with an already-used name (case-insensitive) must be rejected.");
        }

        [TestMethod(DisplayName = nameof(SetStringAndBooleanMetadataValueTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void SetStringAndBooleanMetadataValueTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string stringFieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "sender", MetadataFieldType.String);
            string boolFieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "tax-relevant", MetadataFieldType.Boolean);

            //act
            businessLogicService.SetDocumentMetadataValue(userId, documentId, stringFieldId, "Some Company GmbH");
            businessLogicService.SetDocumentMetadataValue(userId, documentId, boolFieldId, "True");

            //assert
            Document reloaded = businessLogicService.GetDocument(userId, documentId);
            Assert.AreEqual("Some Company GmbH", reloaded.MetadataValues[stringFieldId]);
            //a boolean-value is normalized to its lower-case representation.
            Assert.AreEqual("true", reloaded.MetadataValues[boolFieldId]);
        }

        [TestMethod(DisplayName = nameof(SetInvalidBooleanMetadataValueTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void SetInvalidBooleanMetadataValueTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string boolFieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "tax-relevant", MetadataFieldType.Boolean);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.SetDocumentMetadataValue(userId, documentId, boolFieldId, "maybe");
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Setting a non-boolean value for a boolean-field must be rejected.");
        }

        [TestMethod(DisplayName = nameof(ClearMetadataValueTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void ClearMetadataValueTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "sender", MetadataFieldType.String);
            businessLogicService.SetDocumentMetadataValue(userId, documentId, fieldId, "value");
            Assert.IsTrue(businessLogicService.GetDocument(userId, documentId).MetadataValues.ContainsKey(fieldId));

            //act
            businessLogicService.SetDocumentMetadataValue(userId, documentId, fieldId, null);

            //assert
            Assert.IsFalse(businessLogicService.GetDocument(userId, documentId).MetadataValues.ContainsKey(fieldId), "Clearing a metadata-value must remove it from the document.");
        }

        [TestMethod(DisplayName = nameof(RemoveMetadataFieldRemovesValuesTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void RemoveMetadataFieldRemovesValuesTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "sender", MetadataFieldType.String);
            businessLogicService.SetDocumentMetadataValue(userId, documentId, fieldId, "value");

            //act
            businessLogicService.RemoveMetadataField(userId, fieldId);

            //assert
            Assert.AreEqual(0, businessLogicService.GetMetadataFields(userId, storageLocationId).Count(), "The removed field must no longer be listed.");
            Assert.IsFalse(businessLogicService.GetDocument(userId, documentId).MetadataValues.ContainsKey(fieldId), "The values of a removed field must be removed from the documents.");
        }

        [TestMethod(DisplayName = nameof(RemoveMetadataFieldRequiresModeratorTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void RemoveMetadataFieldRequiresModeratorTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string _);
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "sender", MetadataFieldType.String);
            string otherUserId = "other-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = otherUserId, });

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.RemoveMetadataField(otherUserId, fieldId);
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A user which is not a moderator of the storage-location must not be allowed to remove a metadata-field.");
            Assert.AreEqual(1, businessLogicService.GetMetadataFields(userId, storageLocationId).Count(), "The field must still exist since the removal was rejected.");
        }

        [TestMethod(DisplayName = nameof(GetMetadataFieldsRequiresViewPermissionTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetMetadataFieldsRequiresViewPermissionTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string _);
            businessLogicService.DefineMetadataField(userId, storageLocationId, "sender", MetadataFieldType.String);
            string otherUserId = "other-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = otherUserId, });

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.GetMetadataFields(otherUserId, storageLocationId);
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A user without view-permission on the storage-location must not be allowed to list its metadata-fields.");
        }

        [TestMethod(DisplayName = nameof(GetMetadataFieldsAllowedForSharedViewerTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetMetadataFieldsAllowedForSharedViewerTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string _);
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "sender", MetadataFieldType.String);
            string viewerUserId = "viewer-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = viewerUserId, });
            //grant view-only access; this must be sufficient to list the fields.
            persistence.AuthorizeUserToViewStorageLocation(storageLocationId, viewerUserId);

            //act
            List<MetadataFieldDefinition> fields = businessLogicService.GetMetadataFields(viewerUserId, storageLocationId).ToList();

            //assert
            Assert.AreEqual(1, fields.Count);
            Assert.AreEqual(fieldId, fields[0].Id);
        }

        [TestMethod(DisplayName = nameof(SetDocumentMetadataValueRequiresEditPermissionTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void SetDocumentMetadataValueRequiresEditPermissionTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "sender", MetadataFieldType.String);
            string viewerUserId = "viewer-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = viewerUserId, });
            //grant only view-permission, no edit-permission.
            persistence.AuthorizeUserToViewStorageLocation(storageLocationId, viewerUserId);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.SetDocumentMetadataValue(viewerUserId, documentId, fieldId, "some value");
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A user with only view-permission must not be allowed to set a metadata-value.");
            Assert.IsFalse(businessLogicService.GetDocument(userId, documentId).MetadataValues.ContainsKey(fieldId), "The value must not have been set.");
        }

        [TestMethod(DisplayName = nameof(DefineMetadataFieldOnNonStorageLocationTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void DefineMetadataFieldOnNonStorageLocationTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            string userId = "folder-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = userId, });
            string storageLocationId = persistence.AddStoragLocation("storageLocation");
            persistence.SetOwnerOfStorageLocation(storageLocationId, userId);
            string folderId = businessLogicService.AddFolder(userId, "folder", storageLocationId);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.DefineMetadataField(userId, folderId, "some-field", MetadataFieldType.String);
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Metadata-fields can only be defined for a storage-location, not for a folder.");
        }

        [TestMethod(DisplayName = nameof(DefineMetadataFieldWithEmptyNameTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void DefineMetadataFieldWithEmptyNameTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string _);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.DefineMetadataField(userId, storageLocationId, "   ", MetadataFieldType.String);
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A metadata-field with an empty (whitespace-only) name must be rejected.");
        }

        [TestMethod(DisplayName = nameof(SetDocumentMetadataValueForFieldOfDifferentStorageLocationTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void SetDocumentMetadataValueForFieldOfDifferentStorageLocationTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string otherStorageLocationId = persistence.AddStoragLocation("otherStorageLocation");
            persistence.SetOwnerOfStorageLocation(otherStorageLocationId, userId);
            string fieldOfOtherStorageLocation = businessLogicService.DefineMetadataField(userId, otherStorageLocationId, "sender", MetadataFieldType.String);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.SetDocumentMetadataValue(userId, documentId, fieldOfOtherStorageLocation, "value");
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A metadata-value must not be settable using a field-definition of a different storage-location.");
        }

        [TestMethod(DisplayName = nameof(SoftDeleteRequiresEditPermissionTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void SoftDeleteRequiresEditPermissionTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string otherUserId = "other-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = otherUserId, });

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.SoftDelete(otherUserId, documentId, "not my document");
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A user without edit-permission must not be allowed to soft-delete a document.");
            Assert.IsFalse(persistence.GetDocument(documentId).IsSoftDeleted, "The document must not have been soft-deleted.");
        }

        [TestMethod(DisplayName = nameof(HardDeleteRequiresEditPermissionTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void HardDeleteRequiresEditPermissionTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string otherUserId = "other-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = otherUserId, });

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.HardDelete(otherUserId, documentId, "not my document");
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A user without edit-permission must not be allowed to hard-delete a document.");
            Assert.IsTrue(persistence.IsDocument(documentId), "The document must still exist.");
            Assert.IsFalse(persistence.GetDocument(documentId).IsHardDeleted, "The document must not have been hard-deleted.");
        }

        [TestMethod(DisplayName = nameof(MoveRequiresEditPermissionOnTargetContainerTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void MoveRequiresEditPermissionOnTargetContainerTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string otherStorageLocationId = persistence.AddStoragLocation("otherStorageLocation");
            string otherUserId = "other-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = otherUserId, });
            persistence.SetOwnerOfStorageLocation(otherStorageLocationId, otherUserId);

            //act & assert: the user may edit the document itself (moderator of its storage-location) but has no permission at all on the target container.
            bool threw = false;
            try
            {
                businessLogicService.Move(userId, documentId, otherStorageLocationId);
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Moving into a container the user has no edit-permission for must be rejected even if the user may edit the moved document itself.");
            Assert.AreEqual(storageLocationId, persistence.GetParentIdOfContainee(documentId), "The document must not have been moved.");
        }

        [TestMethod(DisplayName = nameof(GetMetadataFieldsOfDocumentTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetMetadataFieldsOfDocumentTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "sender", MetadataFieldType.String);
            //a field of another storage-location must not be offered for this document.
            string otherStorageLocationId = persistence.AddStoragLocation("otherStorageLocation");
            persistence.SetOwnerOfStorageLocation(otherStorageLocationId, userId);
            businessLogicService.DefineMetadataField(userId, otherStorageLocationId, "sender", MetadataFieldType.String);

            //act
            List<MetadataFieldDefinition> fields = businessLogicService.GetMetadataFieldsOfDocument(userId, documentId).ToList();

            //assert
            Assert.AreEqual(1, fields.Count, "Exactly the fields of the storage-location which contains the document must be returned.");
            Assert.AreEqual(fieldId, fields[0].Id);
        }

        [TestMethod(DisplayName = nameof(GetMetadataFieldsOfDocumentRequiresViewPermissionTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GetMetadataFieldsOfDocumentRequiresViewPermissionTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            businessLogicService.DefineMetadataField(userId, storageLocationId, "sender", MetadataFieldType.String);
            string otherUserId = "other-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = otherUserId, });

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.GetMetadataFieldsOfDocument(otherUserId, documentId);
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A user without view-permission on the document must not be allowed to list the metadata-fields of it.");
        }

        [TestMethod(DisplayName = nameof(CreateAndAssignTagTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void CreateAndAssignTagTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string documentId);

            //act
            string tagId = businessLogicService.CreateTag(userId, "Invoice", new ExtendedColor(198, 40, 40), false);
            businessLogicService.AssignTag(userId, documentId, tagId);

            //assert
            Document reloaded = businessLogicService.GetDocument(userId, documentId);
            Assert.AreEqual(1, reloaded.Tags.Count);
            Assert.AreEqual(tagId, reloaded.Tags.Single().Id);
            Assert.AreEqual("Invoice", reloaded.Tags.Single().Name);
        }

        [TestMethod(DisplayName = nameof(UnassignTagTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void UnassignTagTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string documentId);
            string tagId = businessLogicService.CreateTag(userId, "Invoice", new ExtendedColor(198, 40, 40), false);
            businessLogicService.AssignTag(userId, documentId, tagId);

            //act
            businessLogicService.UnassignTag(userId, documentId, tagId);

            //assert
            Assert.AreEqual(0, businessLogicService.GetDocument(userId, documentId).Tags.Count, "The unassigned tag must not be assigned to the document anymore.");
            Assert.AreEqual(1, businessLogicService.GetTags(userId).Length, "Unassigning a tag must not delete the tag itself.");
        }

        [TestMethod(DisplayName = nameof(CreateTagWithDuplicateNameTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void CreateTagWithDuplicateNameTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string _);
            businessLogicService.CreateTag(userId, "Invoice", new ExtendedColor(198, 40, 40), false);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.CreateTag(userId, "INVOICE", new ExtendedColor(0, 0, 0), false);
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Creating a second tag with an already-used name (case-insensitive) must be rejected.");
            Assert.AreEqual(1, businessLogicService.GetTags(userId).Length);
        }

        [TestMethod(DisplayName = nameof(CreateTagWithEmptyNameTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void CreateTagWithEmptyNameTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string _);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.CreateTag(userId, "   ", new ExtendedColor(0, 0, 0), false);
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A tag with an empty (whitespace-only) name must be rejected.");
        }

        [TestMethod(DisplayName = nameof(AssignTagTwiceTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void AssignTagTwiceTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string documentId);
            string tagId = businessLogicService.CreateTag(userId, "Invoice", new ExtendedColor(198, 40, 40), false);
            businessLogicService.AssignTag(userId, documentId, tagId);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.AssignTag(userId, documentId, tagId);
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Assigning a tag which is already assigned to the document must be rejected.");
        }

        [TestMethod(DisplayName = nameof(UnassignNotAssignedTagTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void UnassignNotAssignedTagTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string documentId);
            string tagId = businessLogicService.CreateTag(userId, "Invoice", new ExtendedColor(198, 40, 40), false);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.UnassignTag(userId, documentId, tagId);
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Unassigning a tag which is not assigned to the document must be rejected.");
        }

        [TestMethod(DisplayName = nameof(AssignTagRequiresEditPermissionTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void AssignTagRequiresEditPermissionTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string tagId = businessLogicService.CreateTag(userId, "Invoice", new ExtendedColor(198, 40, 40), false);
            string viewerUserId = "viewer-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = viewerUserId, });
            //grant only view-permission, no edit-permission.
            persistence.AuthorizeUserToViewStorageLocation(storageLocationId, viewerUserId);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.AssignTag(viewerUserId, documentId, tagId);
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A user with only view-permission must not be allowed to assign a tag to a document.");
            Assert.AreEqual(0, businessLogicService.GetDocument(userId, documentId).Tags.Count, "The tag must not have been assigned.");
        }

        /// <summary>Returns the id of the administrator-user which the initialization created.</summary>
        private static string GetAdministratorUserId(IPersistence persistence)
        {
            return persistence.GetAllUsers().Values.Single(user => user.Name == CodeUnitSpecificConstants.UsernameAdmin).Id;
        }

        /// <summary>Creates a further storage-location which the given user moderates.</summary>
        private static string SetupFurtherStorageLocation(IPersistence persistence, string userId, string name)
        {
            string storageLocationId = persistence.AddStoragLocation(name);
            persistence.SetOwnerOfStorageLocation(storageLocationId, userId);
            return storageLocationId;
        }

        [TestMethod(DisplayName = nameof(CreateGlobalTagRequiresAdministratorTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void CreateGlobalTagRequiresAdministratorTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string _);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.CreateTag(userId, "Invoice", new ExtendedColor(198, 40, 40), true);
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A user who is not an administrator must not be allowed to create a global tag.");
            Assert.AreEqual(0, businessLogicService.GetTags(userId).Length, "The rejected tag must not have been created.");
        }

        [TestMethod(DisplayName = nameof(GlobalTagIsVisibleForEveryUserTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void GlobalTagIsVisibleForEveryUserTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string documentId);
            string administratorUserId = GetAdministratorUserId(persistence);

            //act
            string tagId = businessLogicService.CreateTag(administratorUserId, "Invoice", new ExtendedColor(198, 40, 40), true);

            //assert
            TagDTO[] tagsOfOtherUser = businessLogicService.GetTags(userId);
            Assert.AreEqual(1, tagsOfOtherUser.Length, "A global tag must be visible for every user.");
            Assert.IsNull(tagsOfOtherUser.Single().OwnerUserId, "A global tag must not have an owner.");
            businessLogicService.AssignTag(userId, documentId, tagId);
            Assert.AreEqual(1, businessLogicService.GetDocument(userId, documentId).Tags.Count, "A global tag must be usable by every user.");
        }

        [TestMethod(DisplayName = nameof(TagOfAnotherUserIsNeitherVisibleNorUsableTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TagOfAnotherUserIsNeitherVisibleNorUsableTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string ownerUserId, out string storageLocationId, out string documentId);
            string otherUserId = "other-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = otherUserId, });
            persistence.AuthorizeUserToEditStorageLocation(storageLocationId, otherUserId);
            string tagId = businessLogicService.CreateTag(ownerUserId, "Invoice", new ExtendedColor(198, 40, 40), false);

            //act & assert
            Assert.AreEqual(0, businessLogicService.GetTags(otherUserId).Length, "A tag which belongs to another user must not be visible.");
            bool threw = false;
            try
            {
                businessLogicService.AssignTag(otherUserId, documentId, tagId);
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A tag which belongs to another user must not be assignable.");
        }

        [TestMethod(DisplayName = nameof(UpdateTagTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void UpdateTagTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string documentId);
            string tagId = businessLogicService.CreateTag(userId, "Invoice", new ExtendedColor(198, 40, 40), false);
            businessLogicService.AssignTag(userId, documentId, tagId);

            //act
            businessLogicService.UpdateTag(userId, tagId, "Receipt", new ExtendedColor(0, 128, 0));

            //assert
            TagDTO updatedTag = businessLogicService.GetTags(userId).Single();
            Assert.AreEqual(tagId, updatedTag.Id, "Renaming a tag must not change its id.");
            Assert.AreEqual("Receipt", updatedTag.Name);
            Assert.AreEqual(new ExtendedColor(0, 128, 0).GetRGBString(), updatedTag.ColorCode);
            Assert.AreEqual("Receipt", businessLogicService.GetDocument(userId, documentId).Tags.Single().Name, "The document must show the new name of the tag it is assigned to.");
        }

        [TestMethod(DisplayName = nameof(UpdateTagOfAnotherUserIsRejectedTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void UpdateTagOfAnotherUserIsRejectedTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string ownerUserId, out string _, out string _);
            string otherUserId = "other-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = otherUserId, });
            string tagId = businessLogicService.CreateTag(ownerUserId, "Invoice", new ExtendedColor(198, 40, 40), false);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.UpdateTag(otherUserId, tagId, "Receipt", new ExtendedColor(0, 128, 0));
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A tag which belongs to another user must not be changeable.");
            Assert.AreEqual("Invoice", businessLogicService.GetTags(ownerUserId).Single().Name, "The rejected change must not have been applied.");
        }

        [TestMethod(DisplayName = nameof(UpdateGlobalTagRequiresAdministratorTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void UpdateGlobalTagRequiresAdministratorTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string _);
            string administratorUserId = GetAdministratorUserId(persistence);
            string tagId = businessLogicService.CreateTag(administratorUserId, "Invoice", new ExtendedColor(198, 40, 40), true);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.UpdateTag(userId, tagId, "Receipt", new ExtendedColor(0, 128, 0));
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A global tag must only be changeable by an administrator.");
            businessLogicService.UpdateTag(administratorUserId, tagId, "Receipt", new ExtendedColor(0, 128, 0));
            Assert.AreEqual("Receipt", businessLogicService.GetTags(userId).Single().Name, "An administrator must be able to change a global tag.");
        }

        [TestMethod(DisplayName = nameof(DeleteTagRemovesItsAssignmentsTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void DeleteTagRemovesItsAssignmentsTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string documentId);
            string tagId = businessLogicService.CreateTag(userId, "Invoice", new ExtendedColor(198, 40, 40), false);
            businessLogicService.AssignTag(userId, documentId, tagId);

            //act
            businessLogicService.DeleteTag(userId, tagId);

            //assert
            Assert.AreEqual(0, businessLogicService.GetTags(userId).Length, "The deleted tag must not exist anymore.");
            Assert.AreEqual(0, businessLogicService.GetDocument(userId, documentId).Tags.Count, "The deleted tag must not be assigned to any document anymore.");
        }

        [TestMethod(DisplayName = nameof(DeleteTagOfAnotherUserIsRejectedTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void DeleteTagOfAnotherUserIsRejectedTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string ownerUserId, out string _, out string _);
            string otherUserId = "other-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = otherUserId, });
            string tagId = businessLogicService.CreateTag(ownerUserId, "Invoice", new ExtendedColor(198, 40, 40), false);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.DeleteTag(otherUserId, tagId);
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A tag which belongs to another user must not be deletable.");
            Assert.AreEqual(1, businessLogicService.GetTags(ownerUserId).Length, "The rejected deletion must not have removed the tag.");
        }

        [TestMethod(DisplayName = nameof(TwoUsersCanUseTheSameTagNameTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void TwoUsersCanUseTheSameTagNameTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string firstUserId, out string _, out string _);
            string secondUserId = "second-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = secondUserId, });
            businessLogicService.CreateTag(firstUserId, "Invoice", new ExtendedColor(198, 40, 40), false);

            //act
            businessLogicService.CreateTag(secondUserId, "Invoice", new ExtendedColor(0, 128, 0), false);

            //assert: the name only has to be unambiguous within the tags a single user sees.
            Assert.AreEqual(1, businessLogicService.GetTags(firstUserId).Length);
            Assert.AreEqual(1, businessLogicService.GetTags(secondUserId).Length);
        }

        [TestMethod(DisplayName = nameof(RenameMetadataFieldTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void RenameMetadataFieldTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "contact", MetadataFieldType.String);
            businessLogicService.SetDocumentMetadataValue(userId, documentId, fieldId, "some sender");

            //act
            businessLogicService.RenameMetadataField(userId, fieldId, "sender");

            //assert
            MetadataFieldDefinition field = businessLogicService.GetMetadataFields(userId, storageLocationId).Single();
            Assert.AreEqual(fieldId, field.Id, "Renaming a field must not change its id.");
            Assert.AreEqual("sender", field.Name);
            Assert.AreEqual(MetadataFieldType.String, field.Type, "Renaming a field must not change its type.");
            Assert.AreEqual("some sender", businessLogicService.GetDocument(userId, documentId).MetadataValues[fieldId], "Renaming a field must keep the values the documents hold for it.");
        }

        [TestMethod(DisplayName = nameof(RenameMetadataFieldToAlreadyUsedNameIsRejectedTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void RenameMetadataFieldToAlreadyUsedNameIsRejectedTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string _);
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "contact", MetadataFieldType.String);
            businessLogicService.DefineMetadataField(userId, storageLocationId, "document-type", MetadataFieldType.String);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.RenameMetadataField(userId, fieldId, "DOCUMENT-TYPE");
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "Renaming a field to the name of another field of the same storage-location (case-insensitive) must be rejected.");
            Assert.AreEqual("contact", businessLogicService.GetMetadataFields(userId, storageLocationId).Single(field => field.Id == fieldId).Name);
        }

        [TestMethod(DisplayName = nameof(RenameMetadataFieldRequiresModeratorTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void RenameMetadataFieldRequiresModeratorTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string _);
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "contact", MetadataFieldType.String);
            string editorUserId = "editor-user-" + Guid.NewGuid();
            persistence.AddUser(new User() { Id = editorUserId, });
            persistence.AuthorizeUserToEditStorageLocation(storageLocationId, editorUserId);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.RenameMetadataField(editorUserId, fieldId, "sender");
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A user who is not a moderator of the storage-location must not be allowed to rename one of its metadata-fields.");
        }

        [TestMethod(DisplayName = nameof(SetMetadataValueOfTypeDoubleTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void SetMetadataValueOfTypeDoubleTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "amount", MetadataFieldType.Double);

            //act
            businessLogicService.SetDocumentMetadataValue(userId, documentId, fieldId, "1234.56");

            //assert
            Assert.AreEqual("1234.56", businessLogicService.GetDocument(userId, documentId).MetadataValues[fieldId]);
            bool threw = false;
            try
            {
                businessLogicService.SetDocumentMetadataValue(userId, documentId, fieldId, "not-a-number");
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A value which is not a number must be rejected for a field of the type double.");
        }

        [TestMethod(DisplayName = nameof(SetMetadataValueOfTypeTimestampTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void SetMetadataValueOfTypeTimestampTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "deadline", MetadataFieldType.Timestamp);

            //act
            businessLogicService.SetDocumentMetadataValue(userId, documentId, fieldId, "2026-01-31T12:00:00+01:00");

            //assert: the value is stored in a normalized representation, so that every reader gets the same format back.
            string storedValue = businessLogicService.GetDocument(userId, documentId).MetadataValues[fieldId];
            Assert.AreEqual(new DateTimeOffset(2026, 1, 31, 12, 0, 0, TimeSpan.FromHours(1)), DateTimeOffset.Parse(storedValue, System.Globalization.CultureInfo.InvariantCulture));
            bool threw = false;
            try
            {
                businessLogicService.SetDocumentMetadataValue(userId, documentId, fieldId, "not-a-timestamp");
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A value which is not a timestamp must be rejected for a field of the type timestamp.");
        }

        [TestMethod(DisplayName = nameof(MoveDocumentTransfersItsMetadataValuesTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void MoveDocumentTransfersItsMetadataValuesTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string sourceStorageLocationId, out string documentId);
            string targetStorageLocationId = SetupFurtherStorageLocation(persistence, userId, "target-storageLocation");
            string sourceContactFieldId = businessLogicService.DefineMetadataField(userId, sourceStorageLocationId, "contact", MetadataFieldType.String);
            string sourceOnlyFieldId = businessLogicService.DefineMetadataField(userId, sourceStorageLocationId, "only-in-source", MetadataFieldType.String);
            string targetContactFieldId = businessLogicService.DefineMetadataField(userId, targetStorageLocationId, "contact", MetadataFieldType.String);
            businessLogicService.SetDocumentMetadataValue(userId, documentId, sourceContactFieldId, "some sender");
            businessLogicService.SetDocumentMetadataValue(userId, documentId, sourceOnlyFieldId, "some value");

            //act
            businessLogicService.Move(userId, documentId, targetStorageLocationId);

            //assert
            IDictionary<string, string> valuesAfterTheMove = businessLogicService.GetDocument(userId, documentId).MetadataValues;
            Assert.AreEqual("some sender", valuesAfterTheMove[targetContactFieldId], "The value must be transferred to the field of the new storage-location which has the same name and the same type.");
            Assert.IsFalse(valuesAfterTheMove.ContainsKey(sourceContactFieldId), "The value of the field of the previous storage-location must not be kept.");
            Assert.IsFalse(valuesAfterTheMove.ContainsKey(sourceOnlyFieldId), "A value which has no matching field in the new storage-location must be removed.");
        }

        [TestMethod(DisplayName = nameof(MoveDocumentDoesNotTransferValuesToAFieldOfAnotherTypeTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void MoveDocumentDoesNotTransferValuesToAFieldOfAnotherTypeTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string sourceStorageLocationId, out string documentId);
            string targetStorageLocationId = SetupFurtherStorageLocation(persistence, userId, "target-storageLocation");
            string sourceFieldId = businessLogicService.DefineMetadataField(userId, sourceStorageLocationId, "tax-relevant", MetadataFieldType.String);
            string targetFieldId = businessLogicService.DefineMetadataField(userId, targetStorageLocationId, "tax-relevant", MetadataFieldType.Boolean);
            businessLogicService.SetDocumentMetadataValue(userId, documentId, sourceFieldId, "some text");

            //act
            businessLogicService.Move(userId, documentId, targetStorageLocationId);

            //assert: transferring the value would store a value which does not match the type of the field.
            IDictionary<string, string> valuesAfterTheMove = businessLogicService.GetDocument(userId, documentId).MetadataValues;
            Assert.IsFalse(valuesAfterTheMove.ContainsKey(targetFieldId));
            Assert.IsFalse(valuesAfterTheMove.ContainsKey(sourceFieldId));
        }

        [TestMethod(DisplayName = nameof(HardDeleteIsRejectedWithinTheRetentionPeriodTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void HardDeleteIsRejectedWithinTheRetentionPeriodTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string documentId);
            businessLogicService.SetRetentionDates(userId, documentId, DateTimeOffset.UtcNow.AddYears(10), null);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.HardDelete(userId, documentId, "test");
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A document which is still within its retention-period must not be hard-deletable.");
            Assert.IsFalse(businessLogicService.GetDocumentPreview(userId, documentId).IsHardDeleted, "The document must still exist.");
        }

        [TestMethod(DisplayName = nameof(HardDeleteIsAllowedAfterTheRetentionPeriodTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void HardDeleteIsAllowedAfterTheRetentionPeriodTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string documentId);
            businessLogicService.SetRetentionDates(userId, documentId, DateTimeOffset.UtcNow.AddYears(-1), null);

            //act
            businessLogicService.HardDelete(userId, documentId, "test");

            //assert
            Assert.IsTrue(businessLogicService.GetDocumentPreview(userId, documentId).IsHardDeleted, "A document whose retention-period has ended must be hard-deletable.");
        }

        [TestMethod(DisplayName = nameof(SetRetentionDatesInWrongOrderIsRejectedTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void SetRetentionDatesInWrongOrderIsRejectedTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string _, out string documentId);

            //act & assert
            bool threw = false;
            try
            {
                businessLogicService.SetRetentionDates(userId, documentId, DateTimeOffset.UtcNow.AddYears(10), DateTimeOffset.UtcNow.AddYears(1));
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A retention-period which ends before it begins must be rejected.");
        }

        [TestMethod(DisplayName = nameof(SearchFindsDocumentByItsMetadataValueTest))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void SearchFindsDocumentByItsMetadataValueTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            SetupModeratorWithDocument(persistence, out string userId, out string storageLocationId, out string documentId);
            string fieldId = businessLogicService.DefineMetadataField(userId, storageLocationId, "contact", MetadataFieldType.String);
            businessLogicService.SetDocumentMetadataValue(userId, documentId, fieldId, "Gasworks-company");

            //act
            IList<DocumentPreview> searchResults = businessLogicService.Search(userId, "gasworks");

            //assert
            Assert.AreEqual(1, searchResults.Count, "A document must be findable by the data it is indexed with.");
            Assert.AreEqual(documentId, searchResults.Single().Id);
        }

        //TODO write more testcases for the things which are not allowed to verify the user is really not able to do certain things
    }
}
