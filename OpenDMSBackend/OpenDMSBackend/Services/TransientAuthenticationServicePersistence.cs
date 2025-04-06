using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using OpenDMSBackend.Core.Model.BusinessTypes;

namespace OpenDMSBackend.Core.Services
{
    public class TransientAuthenticationServicePersistence : TransientAuthenticationServicePersistence<User>
    {
        public TransientAuthenticationServicePersistence(ITimeService timeService) : base(timeService)
        {
        }
    }
}
