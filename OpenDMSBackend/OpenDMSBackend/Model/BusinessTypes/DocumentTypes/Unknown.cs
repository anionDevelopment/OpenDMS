using OpenDMSBackend.Core.Services;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.BusinessTypes.DocumentTypes
{
    public class Unknown : DocumentType
    {
        public static DocumentType Instance { get; } = new Unknown();
        public override ISet<string> GetMimeTypes()
        {
            return new HashSet<string>
            {
                "application/octet-stream",
                "application/x-unknown",
                "application/x-unknown-content-type",
                "application/x-unknown-application",
                "application/x-unknown-document",
                "application/x-unknown-file",
                "application/x-unknown-mime-type",
                "application/x-unknown-object",
                "application/x-unknown-type"
            };
        }
        public override byte[] GetPreview(byte[] content)
        {
            throw new System.NotSupportedException();
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
            throw new System.NotSupportedException();
        }
    }
}
