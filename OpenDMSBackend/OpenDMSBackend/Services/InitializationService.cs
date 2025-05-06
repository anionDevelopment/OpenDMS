using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.ConsoleApplication;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Services
{
    public class InitializationService : IInitializationService<CommandlineParameter>
    {
        private readonly IAuthenticationService<Model.BusinessTypes.User> _AuthenticationService;
        private readonly IBusinessLogicService _BusinessLogicService;
        private readonly IGeneralLogger _GeneralLogger;
        private readonly IExampleDataCreator _ExampleDataCreator;
        private readonly IApplicationConstants<CodeUnitSpecificConstants> _Constants;
        private readonly IPersistence _Persistence;
        private readonly IIdGenerator<ulong> _IdGenerator;
        public InitializationService(IAuthenticationService<Model.BusinessTypes.User> authenticationService, IBusinessLogicService businessLogicService, IGeneralLogger generalLogger, IApplicationConstants<CodeUnitSpecificConstants> constants, IExampleDataCreator exampleDataCreator, IPersistence persistence, IIdGenerator<ulong> idGenerator)
        {
            this._AuthenticationService = authenticationService;
            this._BusinessLogicService = businessLogicService;
            this._GeneralLogger = generalLogger;
            this._Constants = constants;
            this._ExampleDataCreator = exampleDataCreator;
            this._Persistence = persistence;
            this._IdGenerator = idGenerator;
        }

        public void Initialize(CommandlineParameter commandlineParameter)
        {
            this._GeneralLogger.Log("Initialize service...", Microsoft.Extensions.Logging.LogLevel.Information);
            string adminUsername = CodeUnitSpecificConstants.UsernameAdmin;
            this._IdGenerator.Reset(this._Persistence.GetLatestReadableId());
            if (!this._BusinessLogicService.UserWithNameExists(adminUsername))
            {
                this._AuthenticationService.EnsureRoleExists(CodeUnitSpecificConstants.RolenameUsers);
                Role usersRole = this._AuthenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameUsers);

                this._AuthenticationService.EnsureRoleExists(CodeUnitSpecificConstants.RolenameAdmins);
                Role adminsRole = this._AuthenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameAdmins);
                adminsRole.InheritedRoles = new HashSet<Role>() { usersRole };
                this._AuthenticationService.UpdateRole(adminsRole);

                string initialAdminPassword = string.IsNullOrWhiteSpace(commandlineParameter.InitialAdminPassword) ? CodeUnitSpecificConstants.UsernameAdmin : commandlineParameter.InitialAdminPassword;//only initial password. should be changed as soon as possible by the admin of course.
                string adminUserId = this._BusinessLogicService.Register(adminUsername, initialAdminPassword);
                this._AuthenticationService.EnsureUserHasRole(adminUserId, adminsRole.Id);

                if (this._Constants.Environment is Development)
                {
                    this._ExampleDataCreator.AddExampleData();
                }
            }
            this._GeneralLogger.Log("Service is initialized.", Microsoft.Extensions.Logging.LogLevel.Information);
        }
    }
}
