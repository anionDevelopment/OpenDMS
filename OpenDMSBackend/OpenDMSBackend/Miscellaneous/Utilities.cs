using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using Microsoft.AspNetCore.StaticFiles;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.DTOs;
using Sprache;
using System.Collections.Generic;
using System;
using System.Linq;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Core.Model.BusinessTypes.DocumentTypes;
using System.IO;
using System.Drawing;

namespace OpenDMSBackend.Core.Miscellaneous
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

        public static DocumentType GetDocumentType(string mimeType)
        {
            foreach (DocumentType documentType in DocumentType.AllDocumentTypes)
            {
                if (documentType.GetMimeTypes().Contains(mimeType))
                {
                    return documentType;
                }
            }
            return Unknown.Instance;
        }
        public static byte[] ResizeImage(byte[] originalImageBytes, int maxWidth, int maxHeight)
        {
            using var inputStream = new MemoryStream(originalImageBytes);
            using var originalImage = Image.FromStream(inputStream);

            // Berechne das Seitenverhältnis
            float aspectRatio = (float)originalImage.Width / originalImage.Height;

            // Bestimme neue Breite und Höhe unter Beibehaltung des Seitenverhältnisses
            int newWidth, newHeight;

            if (originalImage.Width > originalImage.Height)
            {
                // Breite ist größer als Höhe, skaliere basierend auf maxWidth
                newWidth = maxWidth;
                newHeight = (int)(maxWidth / aspectRatio);
            }
            else
            {
                // Höhe ist größer als Breite, skaliere basierend auf maxHeight
                newHeight = maxHeight;
                newWidth = (int)(maxHeight * aspectRatio);
            }

            // Stelle sicher, dass die neue Breite und Höhe die Maximalwerte nicht überschreiten
            if (newWidth > maxWidth)
            {
                newWidth = maxWidth;
                newHeight = (int)(maxWidth / aspectRatio);
            }

            if (newHeight > maxHeight)
            {
                newHeight = maxHeight;
                newWidth = (int)(maxHeight * aspectRatio);
            }

            using var resizedImage = new Bitmap(newWidth, newHeight);
            using var graphics = Graphics.FromImage(resizedImage);

            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(originalImage, 0, 0, newWidth, newHeight);

            using var outputStream = new MemoryStream();
            resizedImage.Save(outputStream, System.Drawing.Imaging.ImageFormat.Jpeg); 
            return outputStream.ToArray();
        }
        internal static bool IsRunningInContainer()
        {
            return "true".Equals(Environment.GetEnvironmentVariable("IsRunningInDockerContainer"));
        }
    }
}
