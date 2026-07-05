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
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.BusinessTypes;
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
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
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
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
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

        [TestMethod(DisplayName = nameof(GetLatestDocumentsTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
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
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
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
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
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
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
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
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
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
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
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
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
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
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
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

        //TODO write testcases for the things which are not allowed to verify the user is really not able to do certain things
    }
}
