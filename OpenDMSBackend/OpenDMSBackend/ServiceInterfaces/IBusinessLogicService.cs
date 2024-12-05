using OpenDMSBackend.Core.Model;
using System.Collections.Generic;
using System.Drawing;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public interface IBusinessLogicService
    {
        #region user
        string Register(string username, string initialAdminPassword);
        bool UserWithNameExists(string username);
        #endregion

        #region Document
        public void AddDocument(string? title, string originalFilename, byte[] content);
        public Document GetDocument(string id);
        public IEnumerable<DocumentPreview> Search(string searchTerm);

        public void CreateTag(string tagName, Color tagColor);
        public void AssignTag(string documentId, string tagId);
        public void UnassignTag(string documentId, string tagId);
        #endregion
    }
}
