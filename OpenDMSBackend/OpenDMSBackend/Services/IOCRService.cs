using System.Collections.Generic;

namespace OpenDMSBackend.Core.Services
{
    public interface IOCRService
    {
        public string GetOCRContent(byte[] documentContentAsPicture, ISet<string> additionalLanguages);
    }
}
