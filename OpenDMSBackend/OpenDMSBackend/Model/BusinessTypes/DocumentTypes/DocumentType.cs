using OpenDMSBackend.Core.Services;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.BusinessTypes.DocumentTypes
{
    public abstract class DocumentType
    {
        public static IList<DocumentType> AllDocumentTypes { get; } = new List<DocumentType>() {
            new PDF(),
            new Picture(),
            new Unknown(),
            new Word(),
        };
        public abstract ISet<string> GetMimeTypes();
        /// <returns>Returns a picture of the first-site of the document.</returns>
        public abstract byte[] GetPreview(byte[] content);
        public abstract string GetOCRContent(byte[] content, ISet<string> additionalLanguages, IOCRService ocrService);
        public override bool Equals(object? obj)
        {
            return obj != null && this.GetType().Equals(obj?.GetType());
        }

        public override int GetHashCode()
        {
            return this.GetType().GetHashCode();
        }

    }
}
