using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.BusinessTypes.DocumentTypes
{
    public class Word : DocumentType
    {
        public static DocumentType Instance { get; } = new Word();

        public override ISet<string> GetMimeTypes()
        {
            return new HashSet<string>
          {
              "application/msword",
              "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
              "application/vnd.ms-word",
              "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
              "application/vnd.ms-word.document.macroEnabled.12",
              "application/vnd.ms-word.template.macroEnabled.12"
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

        public override string GetOCRContent(byte[] content, IOCRService ocrService)
        {
            return Picture.Instance.GetOCRContent(this.ToPicture(content), ocrService);
        }
        public byte[] ToPicture(byte[] content)
        {
            throw new NotImplementedException();
        }
    }
}
