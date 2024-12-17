using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using System.Collections.Generic;
using System.Drawing;

namespace OpenDMSBackend.Core.Services
{
    public interface IBusinessLogicService
    {
        #region user
        string Register(string username, string initialAdminPassword);
        bool UserWithNameExists(string username);
        #endregion

        #region Document
        public void AddDocument(string? title, string ownerId, string originalFilename, byte[] content);
        public Document GetDocument(string id);
        public IEnumerable<DocumentPreview> Search(string userId, string searchTerm);

        public void CreateTag(string tagName, Color tagColor);
        public void AssignTag(string documentId, string tagId);
        public void UnassignTag(string documentId, string tagId);
        public bool UserIsAllowedToViewDocument(string userId, string documentId);
        public TagDTO[] GetAllTags();
        IEnumerable<DocumentPreview> GetLatestDocuments(string userId);
        #endregion
    }
}
