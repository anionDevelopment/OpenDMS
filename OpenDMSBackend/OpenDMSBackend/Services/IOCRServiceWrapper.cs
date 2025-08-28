using System.Collections.Generic;

namespace OpenDMSBackend.Core.Services
{
    public interface IOCRServiceWrapper
    {
        public string GetOCRContent(byte[] documentContentAsPicture, ISet<string> additionalLanguages);
    }
}
