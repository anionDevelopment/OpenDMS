using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using Microsoft.AspNetCore.StaticFiles;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.DTOs;
using Sprache;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Security;
using OpenDMSBackend.Core.Services;

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


        internal static string GetMIMEType(string fileName)
        {
            try
            {
                if (new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string? contentType))
                {
                    if (!string.IsNullOrEmpty(contentType))
                    {
                        return contentType;
                    }
                }
            }
            catch
            {
                GRYLibrary.Core.Misc.Utilities.NoOperation();
            }
            return "application/octet-stream";
        }

        internal static UserInformationDTO GetUserInformation(User user)
        {
            bool isAdmin = user.GetAllRoles().Where(r => r.Name == CodeUnitSpecificConstants.RolenameAdmins).Any();
            return new UserInformationDTO(user.Id, user.Name, isAdmin);
        }

        public static string ToBase64(byte[] content)
        {
            return System.Convert.ToBase64String(content);
        }

        public static byte[] FromBase64(string content)
        {
            return System.Convert.FromBase64String(content);
        }
        public static void DoForContentObject(IPersistence persistence, string contentId, Action<string> isStorageLocationAction, Action<string> isFolderAction, Action<string> isDocumentAction) =>
#pragma warning disable CS8603 // Possible null reference return.
    DoForContentObject<object>(persistence,contentId, (contentId) => { isStorageLocationAction(contentId); return default; }, (contentId) => { isFolderAction(contentId); return default; }, (contentId) => { isDocumentAction(contentId); return default; });
#pragma warning restore CS8603 // Possible null reference return.

        public static T DoForContentObject<T>(IPersistence persistence,string contentId, Func<string, T> isStorageLocationAction, Func<string, T> isFolderAction, Func<string, T> isDocumentAction)
        {
            if (persistence.IsStorageLocation(contentId))
            {
                return isStorageLocationAction(contentId);
            }
            else if (persistence.IsFolder(contentId))
            {
                return isFolderAction(contentId);
            }
            else if (persistence.IsDocument(contentId))
            {
                return isDocumentAction(contentId);
            }
            else
            {
                throw new KeyNotFoundException($"No content found with id '{contentId}'.");
            }
        }
    }
}
