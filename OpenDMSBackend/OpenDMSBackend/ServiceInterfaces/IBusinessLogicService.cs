using OpenDMSBackend.Core.Model;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public interface IBusinessLogicService
    {
        public void AddDocument(string? title, string originalFilename, byte[] content);
        public Document GetDocument(string id);
        public IEnumerable<DocumentPreview> Search(string searchTerm);
        string Register(string username, string initialAdminPassword);
        bool UserWithNameExists(string username);
    }
}
