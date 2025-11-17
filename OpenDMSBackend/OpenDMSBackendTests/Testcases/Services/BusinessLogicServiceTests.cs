using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Services.Interfaces;
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
            IGRYLog logger = GeneralLogger.CreateUsingConsole();
            AuditLog auditLog = new AuditLog(logger);
            IIdGenerator<ulong> idGenerator = new OpenDMSBackend.Core.Services.IdGenerator();
            IPersistence databasePersistence = OpenDMSBackend.Tests.TestUtilities.Utilities.GetTransientPersistence();
            persistence = databasePersistence;
            IApplicationConstants<CodeUnitSpecificConstants> constants = new ApplicationConstants<CodeUnitSpecificConstants>(GeneralConstants.CodeUnitName, GeneralConstants.CodeUnitVersion, Version3.Parse(GeneralConstants.CodeUnitVersion), RunProgram.Instance, QualityCheck.Instance, new CodeUnitSpecificConstants());
            IAuthenticationService<User> authenticationService = new PersistentAuthenticationService(timeService, databasePersistence, logger, constants);
            IGeneralResourceLoader generalResourceLoader = new OpenDMSBackend.Core.Services.GeneralResourceLoader();
            Mock<IOCRServiceClient> ocrServiceClientMock = new Mock<IOCRServiceClient>(MockBehavior.Strict);
            businessLogicService = new BusinessLogicService(databasePersistence, authenticationService, timeService, constants, logger, persistedAPIServerConfiguration, ocrServiceClientMock.Object, idGenerator, generalResourceLoader, auditLog);
            IExampleDataCreator exampleDataCreator = new ExampleDataCreator(businessLogicService, authenticationService);
            initializationService = new InitializationService(authenticationService, businessLogicService, logger, constants, exampleDataCreator, databasePersistence, idGenerator);
        }

        [TestMethod(nameof(DatabaseInitializationTest))]
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

        [TestMethod(nameof(RegisterTest))]
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

        [TestMethod(nameof(GetLatestDocumentsTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public void GetLatestDocumentsTest()
        {
            //arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService, out IPersistence persistence);
            initializationService.Initialize(new CommandlineParameter());
            string user1Id = "user1Id";
            string user2Id = "user2Id";
            Document testDocument1 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 01, 00, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0), new HashSet<string>(), user1Id);
            Document testDocument2 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 02, 00, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0), new HashSet<string>(), user1Id);
            Document testDocument3 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 03, 00, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0), new HashSet<string>(), user1Id);
            Document testDocument4 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 04, 00, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0), new HashSet<string>(), user1Id);
            Document testDocument5 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 05, 00, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0), new HashSet<string>(), user1Id);
            Document testDocument6 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 06, 00, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0), new HashSet<string>(), user2Id);
            Document testDocument7 = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 11, 17, 20, 01, 00, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0), new HashSet<string>(), user1Id);

            persistence.CreateDocument(testDocument1);
            persistence.CreateDocument(testDocument2);
            persistence.CreateDocument(testDocument3);
            persistence.CreateDocument(testDocument4);
            persistence.CreateDocument(testDocument5);
            persistence.CreateDocument(testDocument6);
            persistence.CreateDocument(testDocument7);

            //act
            IEnumerable<DocumentPreview> actual = businessLogicService.GetLatestDocuments(user1Id);

            // assert
            HashSet<string> actualIds = actual.Select(prev => prev.Id).ToHashSet();
            HashSet<string> expectedIds = new HashSet<string>() { testDocument2.Id, testDocument3.Id, testDocument4.Id, testDocument5.Id, testDocument7.Id };
            Assert.IsTrue(actualIds.SetEquals(expectedIds));
        }

        //TODO write testcases for the things which are not allowed to verify the user is really not able to do certain things
    }
}
