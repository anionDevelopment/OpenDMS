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
        /// <summary>Hard-deletes the given content. A document which is still within its retention-period (see <see cref="SetRetentionDates"/>) is not deleted but reported as an error.</summary>
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
        /// <summary>Searches for documents. A document is found by its title, its filenames, its ocr-content and by the data it is indexed with, which are its tags and the values it holds for the metadata-fields of its storage-location.</summary>
        public IList<DocumentPreview> Search(string requesterUserId, string searchTerm);
        /// <summary>Sets the retention-dates of the given document. The requesting user must be allowed to change the document.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="documentId">The id of the document.</param>
        /// <param name="deleteIsNotAllowedBefore">The point in time before which the document must not be hard-deleted, or <see langword="null"/> to remove the deletion-lock.</param>
        /// <param name="mustBeHardDeletedAfter">The point in time after which the scheduled housekeeping hard-deletes the document, or <see langword="null"/> to remove the automatic deletion.</param>
        public void SetRetentionDates(string requesterUserId, string documentId, System.DateTimeOffset? deleteIsNotAllowedBefore, System.DateTimeOffset? mustBeHardDeletedAfter);
        /// <remarks>
        /// <paramref name="contentId"/> can be an id of any existing <see cref="IContent"/>-object.
        /// </remarks>
        public bool UserIsAllowedToViewContent(string userId, string contentId);
        /// <remarks>
        /// <paramref name="contentId"/> can be an id of any existing <see cref="IContent"/>-object.
        /// </remarks>
        public bool UserIsAllowedToEditContent(string userId, string contentId);
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

        /// <summary>Returns the color-scheme the given user chose.</summary>
        /// <param name="userId">The id of the user.</param>
        /// <returns>"system", "light" or "dark". A user who did not choose a color-scheme yet gets "system", which follows the setting of the operating-system of that user.</returns>
        public string GetThemeOfUser(string userId);

        /// <summary>Sets the color-scheme of the given user.</summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="theme">"system", "light" or "dark".</param>
        public void SetThemeOfUser(string userId, string theme);
        #endregion

        #region Storage
        public bool UserIsAllowedToViewStorageLocation(string userId, string storageLocationId);
        public bool UserIsAllowedToViewFolder(string userId, string folderId);
        public bool UserIsAllowedToViewDocument(string userId, string documentId);
        /// <summary>
        /// Moves the given containee into the given container.
        /// When the move changes the storage-location, the metadata-values of every moved document are transferred to the field of the new storage-location which has the same name and the same type, and are removed when the new storage-location has no such field.
        /// </summary>
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

        #region Metadata-fields
        /// <summary>Defines a new custom metadata-field for the given storage-location. Only a moderator of the storage-location may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="storageLocationId">The id of the storage-location the field is defined for.</param>
        /// <param name="name">The display-name of the field (must be unique within the storage-location).</param>
        /// <param name="type">The value-type of the field (string, boolean, double or timestamp).</param>
        /// <returns>The id of the created field-definition.</returns>
        public string DefineMetadataField(string requesterUserId, string storageLocationId, string name, MetadataFieldType type);
        /// <summary>Renames an existing custom metadata-field. Only a moderator of the field's storage-location may do this. The type of a field can not be changed, because the values which the documents already hold for it were validated against it.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="fieldDefinitionId">The id of the field-definition to rename.</param>
        /// <param name="newName">The new display-name of the field (must be unique within the storage-location).</param>
        public void RenameMetadataField(string requesterUserId, string fieldDefinitionId, string newName);
        /// <summary>Removes the given custom metadata-field-definition together with all values documents hold for it. Only a moderator of the field's storage-location may do this.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="fieldDefinitionId">The id of the field-definition to remove.</param>
        public void RemoveMetadataField(string requesterUserId, string fieldDefinitionId);
        /// <summary>Returns all custom metadata-fields defined for the given storage-location. The requesting user must be allowed to view the storage-location.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="storageLocationId">The id of the storage-location.</param>
        /// <returns>The field-definitions of the storage-location.</returns>
        public IEnumerable<MetadataFieldDefinition> GetMetadataFields(string requesterUserId, string storageLocationId);
        /// <summary>Returns all custom metadata-fields which are applicable to the given document, which are the fields defined for the storage-location the document is contained in. The requesting user must be allowed to view the document.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="documentId">The id of the document.</param>
        /// <returns>The field-definitions which the document can hold a value for.</returns>
        public IEnumerable<MetadataFieldDefinition> GetMetadataFieldsOfDocument(string requesterUserId, string documentId);
        /// <summary>Sets (or, when <paramref name="value"/> is <see langword="null"/>, clears) the value the given document holds for the given metadata-field. The requesting user must be allowed to change the document and the field must be defined for the document's storage-location.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="documentId">The id of the document.</param>
        /// <param name="fieldDefinitionId">The id of the metadata-field-definition.</param>
        /// <param name="value">The value to set, or <see langword="null"/> to clear the value. The value must match the type of the field: a boolean ("true"/"false"), a number in the invariant culture (for example "1234.56") or a timestamp in the iso-8601-format (for example "2026-01-31T12:00:00+01:00").</param>
        public void SetDocumentMetadataValue(string requesterUserId, string documentId, string fieldDefinitionId, string? value);
        #endregion

        #region Tags
        /// <summary>Creates a new tag which can afterwards be assigned to documents. Every authenticated user may create a tag which belongs to them; only an administrator may create a global tag which every user can use.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="tagName">The display-name of the tag (must not be empty and must not be used by another tag which is visible for the requesting user yet).</param>
        /// <param name="tagColor">The color the user-interface shows the tag in.</param>
        /// <param name="isGlobal">Whether the tag is created as a global tag (only allowed for an administrator) instead of a tag which belongs to the requesting user.</param>
        /// <returns>The id of the created tag.</returns>
        public string CreateTag(string requesterUserId, string tagName, ExtendedColor tagColor, bool isGlobal);
        /// <summary>Returns the tags which the given user can use, which are the global tags and the tags the user owns.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        public TagDTO[] GetTags(string requesterUserId);
        /// <summary>Changes the name and the color of an existing tag. A global tag may only be changed by an administrator and a tag which belongs to a user only by that user.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="tagId">The id of the tag to change.</param>
        /// <param name="newTagName">The new display-name of the tag.</param>
        /// <param name="newTagColor">The new color the user-interface shows the tag in.</param>
        public void UpdateTag(string requesterUserId, string tagId, string newTagName, ExtendedColor newTagColor);
        /// <summary>Deletes an existing tag together with all of its assignments to documents. A global tag may only be deleted by an administrator and a tag which belongs to a user only by that user.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="tagId">The id of the tag to delete.</param>
        public void DeleteTag(string requesterUserId, string tagId);
        /// <summary>Assigns an existing tag to an existing document. The requesting user must be allowed to change the document.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="documentId">The id of the document.</param>
        /// <param name="tagId">The id of the tag to assign.</param>
        public void AssignTag(string requesterUserId, string documentId, string tagId);
        /// <summary>Removes the assignment of a tag from a document. The requesting user must be allowed to change the document.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="documentId">The id of the document.</param>
        /// <param name="tagId">The id of the tag to unassign.</param>
        public void UnassignTag(string requesterUserId, string documentId, string tagId);
        #endregion

        #endregion
    }
}
