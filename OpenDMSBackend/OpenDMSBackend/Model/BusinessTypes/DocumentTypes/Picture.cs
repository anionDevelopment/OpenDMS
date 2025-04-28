using OpenDMSBackend.Core.Services;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.BusinessTypes.DocumentTypes
{
    public class Picture : DocumentType
    {
        public static DocumentType Instance { get; } = new Picture();
        public override ISet<string> GetMimeTypes()
        {
            return new HashSet<string>()
            {
                "image/jpeg",
                "image/png",
                "image/gif",
                "image/bmp",
                "image/tiff",
                "image/webp"
            };
        }
        public override byte[] GetPreview(byte[] content)
        {
            int factor = 4;
            return Miscellaneous.Utilities.ResizeImage(content, 210 * factor, 297 * factor);//aspect ratio of DIN-A4
        }
        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string GetOCRContent(byte[] content, IOCRService ocrService)
        {
           return ocrService.GetOCRContent(content);
        }
    }
}
