using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public class OpenDMSBackendTransientAuthenticationServicePersistence : TransientAuthenticationServicePersistence<Model.User>
    {
        public OpenDMSBackendTransientAuthenticationServicePersistence(ITimeService timeService) : base(timeService)
        {
        }
    }
}
