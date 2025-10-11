using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.BusinessTypes;

namespace OpenDMSBackend.Core.Services
{
    /// <summary>
    /// Represetns a authenticationservice.
    /// </summary>
    public class OpenDMSBackendTransientAuthenticationService : TransientAuthenticationService<User>
    {
        public OpenDMSBackendTransientAuthenticationService(ITimeService timeService, ITransientAuthenticationServicePersistence<User> transientAuthenticationServicePersistence, IGeneralLogger logger, IApplicationConstants<CodeUnitSpecificConstants> constants, IAuthenticationServiceSettings authenticationServiceSettings) : base(timeService, transientAuthenticationServicePersistence, authenticationServiceSettings)
        {
        }
    }
}
