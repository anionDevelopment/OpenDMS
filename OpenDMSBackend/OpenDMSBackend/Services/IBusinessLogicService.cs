using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Services
{
    public interface IBusinessLogicService
    {
        #region user
        /// <returns>Returns the id of the created user.</returns>
        public string Register(string username, string password);
        public bool UserWithNameExists(string username);
        public bool UserIsAdministrator(string userId);
        public System.Collections.Generic.IEnumerable<UserOverviewDTO> GetAllUsersWithRoles(string requesterUserId);
        public System.Collections.Generic.IEnumerable<string> GetAllRoleNames(string requesterUserId);
        public void SetRolesOfUser(string requesterUserId, string targetUserId, System.Collections.Generic.ISet<string> roleNames);
        public User GetUser(string userId);
        public AccessToken Login(string username, string password);
        #endregion

        public void Housekeeping();//TODO call this function regulary

        #region BusinessLogic

        public void SoftDelete(string? requesterUserId, string containerOrContaineeId, string reason);
        public void HardDelete(string? requesterUserId, string containerOrContaineeId, string reason);

        public void RemoveEntireContent(string requesterUserId, string containerId,string reason);

        #region Document
        /// <returns>Returns the id of the created document.</returns>
        public string AddDocument(string? requesterUserId, string? title, string containerId, string originalFilename, byte[] content,  string groupOfBusinessOwner, ISet<string> additionalOCRLanguages);
        /// <summary>Uploads a new version of an existing document. The new version is stored as a regular document in the same folder as the old one; both documents stay unchanged and are only linked as a version-relationship.</summary>
        /// <returns>Returns the id of the newly created document (the new version).</returns>
        public string UploadNewVersion(string? requesterUserId, string oldDocumentId, string? title, string originalFilename, byte[] content, string groupOfBusinessOwner, ISet<string> additionalOCRLanguages);
        /// <summary>Returns the complete version-history (from the oldest to the newest version) of the version-chain the given document belongs to.</summary>
        public IEnumerable<DocumentPreview> GetVersionHistory(string requesterUserId, string documentId);
        public void Update(string requesterUserId, Document updatedDocument);
        public Document GetDocument(string requesterUserId, string id);
        public IList<DocumentPreview> Search(string requesterUserId, string searchTerm);
        public void CreateTag(string requesterUserId, string tagName, ExtendedColor tagColor);
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
        /// <summary>Generates the short and the long AI-summary of the specified document and stores them.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="documentId">The id of the document to summarize.</param>
        public void GenerateAISummary(string requesterUserId, string documentId);
        #endregion

        #region Settings
        /// <summary>Returns whether an AI-summary is generated automatically whenever a document is added or changed.</summary>
        public bool GetAutoGenerateAISummary();
        /// <summary>Sets whether an AI-summary is generated automatically whenever a document is added or changed. Only administrators are allowed to change this setting.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="enabled">Whether the automatic generation should be enabled.</param>
        public void SetAutoGenerateAISummary(string requesterUserId, bool enabled);
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
        public void AuthorizeUserToEditStorageLocation(string requesterUserId, string storageLocationId, string editUserId);
        public void UnauthorizeUserToEditStorageLocation(string requesterUserId, string storageLocationId, string editUserId);
        public void AddModerator(string requesterUserId, string contentId, string newModeratorUserId);
        public void RemoveModerator(string requesterUserId, string contentId, string moderatorUserId);
        public System.Collections.Generic.IEnumerable<string> GetModerators(string requesterUserId, string contentId);
        public IEnumerable<StorageLocation> GetAllViewableStorageLocations(string requesterUserId);
        public Folder GetFolder(string requesterUserId, string folderId);
        public Document GetDocumentFromReadableId(string requesterUserId, uint readableDocumentId);
        public StorageLocation GetStorageLocation(string requesterUserId, string storageLocationId);
        public void UpdateDocumentTitle(string requesterUserId, string documentId, string newTitle);

        #endregion

        #endregion
    }
}
