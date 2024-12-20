namespace OpenDMSBackend.Core.Services
{
    public interface IOCRService
    {
       public string GetOCRContent(byte[] documentContent,string mimeType);
    }
}
