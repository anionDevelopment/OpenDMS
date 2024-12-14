using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.DTOs;
using System;
using System.Linq;

namespace OpenDMSBackend.Core.Miscellaneous
{
    internal static class Utilities
    {
        internal static byte[] GeneratePreview(byte[] documentContent)
        {
            throw new NotImplementedException();
        }

        internal static GRYEnvironment GetEnvironmentTargetType()
        {
#if Development
            return Development.Instance;
#elif QualityCheck
            return QualityCheck.Instance;
#elif Productive
            return Productive.Instance;
#else
            throw new System.Collections.Generic.KeyNotFoundException("Unknown environmenttargettype.");
#endif
        }

        internal static UserInformationDTO GetUserInformation(User user)
        {
            bool isAdmin = user.GetAllRoles().Where(r => r.Name == CodeUnitSpecificConstants.RolenameAdmins).Any();
            return new UserInformationDTO(user.Id, user.Name, isAdmin);
        }
    }
}
