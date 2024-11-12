using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using OpenDMSBackend.Core.Constants;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    /// <summary>
    /// Represetns a authenticationservice.
    /// </summary>
    public class OpenDMSBackendTransientAuthenticationService : TransientAuthenticationService<Model.User>
    {
        public OpenDMSBackendTransientAuthenticationService( ITimeService timeService, ITransientAuthenticationServicePersistence<Model.User> transientAuthenticationServicePersistence, IGeneralLogger logger, IApplicationConstants<CodeUnitSpecificConstants> constants) : base(timeService, transientAuthenticationServicePersistence)
        {
        }
    }
}
