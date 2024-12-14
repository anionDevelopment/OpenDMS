using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using OpenDMSBackend.Core.Model.BusinessTypes;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public class OpenDMSBackendTransientAuthenticationServicePersistence : TransientAuthenticationServicePersistence<User>
    {
        public OpenDMSBackendTransientAuthenticationServicePersistence(ITimeService timeService) : base(timeService)
        {
        }
    }
}
