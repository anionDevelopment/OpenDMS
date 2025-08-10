using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.BusinessTypes.DocumentTypes
{
    public class PDF : DocumentType
    {
        public static DocumentType Instance { get; } = new PDF();


        public override ISet<string> GetMimeTypes()
        {
            return new HashSet<string>
            {
                "application/pdf",
                "application/x-pdf",
                "application/acrobat",
                "text/pdf",
                "text/x-pdf",
                "image/pdf"
            };
        }

        public override byte[] GetPreview(byte[] content)
        {
            return Picture.Instance.GetPreview(this.ToPicture(content));
        }
        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string GetOCRContent(byte[] content, ISet<string> additionalLanguages, IOCRService ocrService)
        {
            return Picture.Instance.GetOCRContent(this.ToPicture(content),additionalLanguages, ocrService);
        }
        public byte[] ToPicture(byte[] content)
        {
            throw new NotImplementedException();
        }
    }
}
