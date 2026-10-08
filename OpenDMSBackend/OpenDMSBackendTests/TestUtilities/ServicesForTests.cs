using GRYLibrary.Core.APIServer;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Services.Res;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using Moq;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Services;

namespace OpenDMSBackend.Tests.TestUtilities
{
    /// <summary>Builds the business-logic-services of the backend on top of a given persistence, so that the same business-logic can be tested against every persistence.</summary>
    internal static class ServicesForTests
    {
        internal static void CreateServices(IPersistence persistence, ITimeService timeService, bool registrationIsEnabled, out IBusinessLogicService businessLogicService, out IInitializationService<CommandlineParameter> initializationService)
        {
            IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> persistedAPIServerConfiguration = new PersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>();
            persistedAPIServerConfiguration.ApplicationSpecificConfiguration = new CodeUnitSpecificConfiguration();
            persistedAPIServerConfiguration.ApplicationSpecificConfiguration.RegistrationIsEnabled = registrationIsEnabled;
            ApplicationConstants<CodeUnitSpecificConstants> constants = new ApplicationConstants<CodeUnitSpecificConstants>(GeneralConstants.CodeUnitName, GeneralConstants.CodeUnitVersion, Version3.Parse(GeneralConstants.CodeUnitVersion), TestRun.Instance, OpenDMSBackend.Core.Misc.Utilities.GetEnvironmentTargetType(), new CodeUnitSpecificConstants());
            constants.BaseFolder = APIServer<CodeUnitSpecificConstants, PersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>, CommandlineParameter>.GetDefaultBaseFolder(constants, true);
            IServerLog logger = new ServerLog(new GRYLogConfiguration(true), constants.GetLogFolder());
            IAuditLog auditLog = new AuditLog(new GRYLogConfiguration(true), constants.GetLogFolder());
            IIdGenerator<ulong> idGenerator = new OpenDMSBackend.Core.Services.IdGenerator();
            IAuthenticationService<User> authenticationService = new PersistentAuthenticationService(timeService, persistence, logger, constants);
            IGeneralResourceLoader generalResourceLoader = new OpenDMSBackend.Core.Services.GeneralResourceLoader();
            Mock<IOCRServiceClient> ocrServiceClientMock = new Mock<IOCRServiceClient>(MockBehavior.Strict);
            businessLogicService = new BusinessLogicService(persistence, authenticationService, timeService, constants, logger, persistedAPIServerConfiguration, ocrServiceClientMock.Object, new OpenDMSBackend.Core.Services.AISummaryServiceClientMock(), idGenerator, generalResourceLoader, auditLog);
            IExampleDataCreator exampleDataCreator = new ExampleDataCreator(businessLogicService, authenticationService);
            initializationService = new InitializationService(authenticationService, businessLogicService, logger, constants, exampleDataCreator, persistence, idGenerator);
        }
    }
}
