using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.ServiceInterfaces;
using System;

namespace OpenDMSBackend.Core.Miscellaneous
{
    public class ExampleDataCreator : IExampleDataCreator
    {
        private readonly IBusinessLogicService _BusinessLogicService;
        private readonly IPersistence _Persistence;
        private readonly IAuthenticationService<Model.User> _AuthenticationService;
        private readonly ITimeService _TimeService;
        private readonly IGeneralLogger _Logger;
        private readonly IApplicationConstants<CodeUnitSpecificConstants> _Constants;
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _Configuration;
        private static readonly Random _Random = new Random();

        public ExampleDataCreator(IPersistence persistence, IAuthenticationService<Model.User> authenticationService, ITimeService timeService, IGeneralLogger logger, IApplicationConstants<CodeUnitSpecificConstants> constants, IBusinessLogicService businessLogicService, IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration)
        {
            this._Persistence = persistence;
            this._AuthenticationService = authenticationService;
            this._TimeService = timeService;
            this._Logger = logger;
            this._Constants = constants;
            this._BusinessLogicService = businessLogicService;
            this._Configuration = configuration;
        }

        public void AddExampleData()
        {
            //TODO add example data
        }
    }
}
