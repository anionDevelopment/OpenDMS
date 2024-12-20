using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.DTOs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenDMSBackend.Core.Miscellaneous
{
    internal static class Utilities
    {
        internal static byte[] GeneratePreview(byte[] documentContent, string mimeType)
        {
            return new byte[] { 1, 2, 3 };//TODO implement function
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

        private static readonly IList<string> ExtensionsForImage = new List<string>() { "jpg", "jpeg", "png", "bmp", "gif", "tiff", "svg" };
        private static readonly IList<string> ExtensionsForPlainText = new List<string>() { "jpg", "jpeg", "png", "bmp", "gif", "tiff", "svg" };
        internal static string GetMIMEType(string originalFilename)
        {
            string extension = Path.GetExtension(originalFilename.ToLower())[1..];
            if (extension == "pdf")
            {
                return "application/pdf";
            }
            if (ExtensionsForImage.Contains(extension))
            {
                return "image/jpeg";
            }
            if (ExtensionsForPlainText.Contains(extension))
            {
                return "text/plain";
            }
            return "application/octet-stream";
        }

        internal static UserInformationDTO GetUserInformation(User user)
        {
            bool isAdmin = user.GetAllRoles().Where(r => r.Name == CodeUnitSpecificConstants.RolenameAdmins).Any();
            return new UserInformationDTO(user.Id, user.Name, isAdmin);
        }
    }
}
