using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Services
{
    public interface IBusinessLogicService
    {
        #region user
        /// <returns>Returns the id of the created user.</returns>
        string Register(string username, string initialAdminPassword);
        bool UserWithNameExists(string username);
        bool UserIsAdministrator(string userId);
        #endregion

        public void Housekeeping();//TODO call this function regulary

        #region BusinessLogic

        public void SoftDelete(string requesterUserId, string containerOrContaineeId);
        public void HardDelete(string requesterUserId, string containerOrContaineeId);

        public void RemoveEntireContent(string requesterUserId, string containerId);

        #region Document
        /// <returns>Returns the id of the created document.</returns>
        public string AddDocument(string requesterUserId, string? title, string containerId, string originalFilename, byte[] content,  string groupOfBusinessOwner, ISet<string> additionalOCRLanguages);
        public void Update(string requesterUserId, Document updatedDocument);
        public Document GetDocument(string requesterUserId, string id);
        public IList<DocumentPreview> Search(string requesterUserId, string searchTerm);
        public void CreateTag(string tagName, ExtendedColor tagColor);
        /// <remarks>
        /// <paramref name="contentId"/> can be an id of any existing <see cref="IContent"/>-object.
        /// </remarks>
        public bool UserIsAllowedToViewContent(string userId, string contentId);
        /// <remarks>
        /// <paramref name="contentId"/> can be an id of any existing <see cref="IContent"/>-object.
        /// </remarks>
        public bool UserIsAllowedToEditContent(string userId, string contentId);
        public TagDTO[] GetAllTags();
        IEnumerable<DocumentPreview> GetLatestDocuments(string requesterUserId);
        public DocumentPreview GetDocumentPreview(string requesterUserId, string documentId);
        #endregion

        #region Storage
        public bool UserIsAllowedToViewStorageLocation(string userId, string storageLocationId);
        public bool UserIsAllowedToViewFolder(string userId, string folderId);
        public bool UserIsAllowedToViewDocument(string userId, string documentId);
        public void Move(string requesterUserId, string containeeIdToMove, string targetContainerId);
        /// <returns>Returns the id of the created storage-location.</returns>
        public string AddStorageLocation(string requesterUserId, string name);
        /// <returns>Returns the id of the created folder.</returns>
        public string AddFolder(string requesterUserId, string name, string parentContainerId);
        public void Rename(string requesterUserId, string containerId, string newName);
        public void AuthorizeUserToViewStorageLocation(string requesterUserId, string storageLocationId, string sharedWithUserId);
        public void UnauthorizeUserToViewStorageLocation(string requesterUserId, string storageLocationId, string sharedWithUserId);
        public IEnumerable<StorageLocation> GetAllViewableStorageLocations(string requesterUserId);
        public Folder GetFolder(string requesterUserId, string folderId);
        public Document GetDocumentFromReadableId(string requesterUserId, uint readableDocumentId);

        #endregion

        #endregion
    }
}
