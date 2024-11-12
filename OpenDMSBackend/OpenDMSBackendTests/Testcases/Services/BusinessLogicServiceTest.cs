using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Services.TS;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Database;
using OpenDMSBackend.Core.Miscellaneous;
using OpenDMSBackend.Core.Model;
using OpenDMSBackend.Core.ServiceInterfaces;
using OpenDMSBackend.Tests.TestUtilities;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using OpenDMSBackend.Core.Configuration;
using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.Logging.GRYLogger;

namespace OpenDMSBackend.Tests.Testcases.Services
{
    [TestClass]
    public class BusinessLogicServiceTest
    {
        private void InitializeServices(bool registrationIsEnabled, out IBusinessLogicService businessLogicService, out DatabaseTestFramework databaseTestFramework, out IInitializationService initializationService, out IPersistence persistence)
        {
            databaseTestFramework = new DatabaseTestFramework();
            IDatabaseManager databaseManager = new DatabaseManager();
            GRYMigrator.DoAllMigrations(databaseTestFramework.MySqlConnection, databaseManager);
            DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseMySql(databaseTestFramework.ConnectionString, ServerVersion.AutoDetect(databaseTestFramework.ConnectionString));
            ITimeService timeService = new TimeService();
            IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> persistedAPIServerConfiguration = new PersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>();
            persistedAPIServerConfiguration.ApplicationSpecificConfiguration = new CodeUnitSpecificConfiguration();
            persistedAPIServerConfiguration.ApplicationSpecificConfiguration.RegistrationIsEnabled = registrationIsEnabled;
            IGRYLog logger = GeneralLogger.CreateUsingConsole();
            DatabasePersistence databasePersistence = new DatabasePersistence(optionsBuilder.Options, logger, timeService, databaseManager, logger);
            persistence = databasePersistence;
            persistence.Reset();
            IApplicationConstants<CodeUnitSpecificConstants> constants = new ApplicationConstants<CodeUnitSpecificConstants>(GeneralConstants.CodeUnitName, GeneralConstants.CodeUnitVersion, Version3.Parse(GeneralConstants.CodeUnitVersion), RunProgram.Instance, QualityCheck.Instance, new CodeUnitSpecificConstants());
            IAuthenticationService<User> authenticationService = new OpenDMSBackendPersistentAuthenticationService( timeService, databasePersistence, logger, constants);
            businessLogicService = new BusinessLogicService( databasePersistence, authenticationService, timeService,   constants, logger, persistedAPIServerConfiguration);
            IExampleDataCreator exampleDataCreator = new ExampleDataCreator(databasePersistence, authenticationService, timeService,  logger, constants, businessLogicService, persistedAPIServerConfiguration);
            initializationService = new InitializationService(authenticationService, businessLogicService, logger, constants, exampleDataCreator);
        }

        [TestMethod(nameof(DatabaseInitializationTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public void DatabaseInitializationTest()
        {
            // arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out DatabaseTestFramework databaseTestFramework, out IInitializationService initializationService, out IPersistence persistence);
            using (databaseTestFramework)
            {
                string adminUserName = CodeUnitSpecificConstants.UsernameAdmin;

                // act
                initializationService.Initialize();

                // assert
                Assert.IsTrue(persistence.UserExistsByName(adminUserName));
                Assert.IsTrue(businessLogicService.UserExists(adminUserName));
                // TODO add more assertions
            }
        }

        [TestMethod(nameof(RegisterTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public void RegisterTest()
        {
            // arrange
            this.InitializeServices(true, out IBusinessLogicService businessLogicService, out DatabaseTestFramework databaseTestFramework, out IInitializationService initializationService, out IPersistence persistence);
            using (databaseTestFramework)
            {
                initializationService.Initialize();
                string user = "someuser";
                string password = "somepassword";
                Assert.IsFalse(persistence.UserExistsByName(user));

                // act
                string userId = businessLogicService.Register(user, password);

                // assert
                Assert.IsTrue(persistence.UserWithIdExists(userId));
                Assert.IsTrue(persistence.UserExistsByName(user));
                Assert.IsTrue(businessLogicService.UserExists(user));
                // TODO add more assertions
            }
        }

        //TODO write testcases for the things which are not allowed to verify the user is really not able to do certain things
    }
}
