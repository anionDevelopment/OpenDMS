using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.DTOs;
using Sprache;
using System.Collections.Generic;
using System;
using System.Linq;
using OpenDMSBackend.Core.Services;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

namespace OpenDMSBackend.Core.Misc
{
    internal static class Utilities
    {

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

        internal static string LanguagesListToString(ISet<string> languages)
        {
            return string.Join(",", languages);
        }
        internal static ISet<string> StringToLanguagesList(string languages)
        {
            if (languages == null || string.Empty == languages)
            {
                return new HashSet<string>();
            }
            else if (languages.Contains(","))
            {
                return new HashSet<string>(languages.Split(","));
            }
            else
            {
                return new HashSet<string>() { languages };
            }
        }

        internal static UserInformationDTO GetUserInformation(User user)
        {
            bool isAdmin = user.GetAllRoles().Where(r => r.Name == CodeUnitSpecificConstants.RolenameAdmins).Any();
            return new UserInformationDTO(user.Id, user.Name, isAdmin);
        }

        public static string ToBase64(byte[] content)
        {
            return Convert.ToBase64String(content);
        }

        public static byte[] FromBase64(string content)
        {
            return Convert.FromBase64String(content);
        }
        public static void DoForContentObject(IPersistence persistence, string contentId, Action<string>? isStorageLocationAction, Action<string>? isFolderAction, Action<string>? isDocumentAction) =>
#pragma warning disable CS8603 // Possible null reference return.
    DoForContentObject<object>(persistence, contentId, (contentId) =>
    {
        isStorageLocationAction?.Invoke(contentId);
        ;
        return default;
    }, (contentId) =>
    {
        isFolderAction?.Invoke(contentId);
        return default;
    }, (contentId) =>
    {
        isDocumentAction?.Invoke(contentId);
        return default;
    });
#pragma warning restore CS8603 // Possible null reference return.

        public static T DoForContentObject<T>(IPersistence persistence, string contentId, Func<string, T> isStorageLocationAction, Func<string, T> isFolderAction, Func<string, T> isDocumentAction)
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

        public static byte[] ResizeImage(byte[] originalImageBytes, int maxWidth, int maxHeight)
        {
            using MemoryStream inputStream = new MemoryStream(originalImageBytes);
            using Image<Rgba32> image = Image.Load<Rgba32>(inputStream);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(maxWidth, maxHeight)
            }));

            using MemoryStream outputStream = new MemoryStream();
            image.Save(outputStream, GRYLibrary.Core.Misc.Utilities.GetValue(image.Metadata.DecodedImageFormat));
            return outputStream.ToArray();
        }
        internal static bool IsRunningInContainer()
        {
            return "true".Equals(Environment.GetEnvironmentVariable("IsRunningInDockerContainer"));
        }
    }
}
