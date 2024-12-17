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

        #region BusinessLogic

        public void Delete(string requesterUserId, string containerOrContaineeId);

        #region Document
        public void AddDocument(string requesterUserId, string? title, string containerId, string originalFilename, byte[] content);
        public void Update(string requesterUserId, Document updatedDocument);
        public Document GetDocument(string requesterUserId, string id);
        public IEnumerable<DocumentPreview> Search(string requesterUserId, string searchTerm);
        public void CreateTag(string tagName, Color tagColor);
        public bool UserIsAllowedToViewDocument(string userId, string documentId);
        public bool UserIsAllowedToEditDocument(string userId, string documentId);
        public TagDTO[] GetAllTags();
        IEnumerable<DocumentPreview> GetLatestDocuments(string requesterUserId);
        #endregion

        #region Storage
        public void Move(string requesterUserId, string containeeIdToMove,string targetContainerId);
        public void AddStorageLocation(string requesterUserId, string name);
        public void AddFolder(string requesterUserId, string name, string parentContainerId);
        public void Rename(string requesterUserId, string containerId, string newName);
        public void AuthorizeUserToViewStorageLocation(string requesterUserId, string storageLocationId, string sharedWithUserId);
        public void UnauthorizeUserToViewStorageLocation(string requesterUserId, string storageLocationId, string sharedWithUserId);
        #endregion

        #endregion
    }
}
