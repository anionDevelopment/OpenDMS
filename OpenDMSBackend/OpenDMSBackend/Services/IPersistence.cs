using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Utilities;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Services
{
    public interface IPersistence :  IAuthenticationServicePersistence<Model.BusinessTypes.User>, IReconnectableDatabase
    {
        /// <summary>Returns the <see cref="IContainee"/> with the specified id.</summary>
        /// <param name="containeeId">The id of the containee to retrieve.</param>
        /// <returns>The <see cref="IContainee"/> matching the given id.</returns>
        public IContainee GetContaineeById(string containeeId);

        /// <summary>Returns the <see cref="IContainer"/> with the specified id.</summary>
        /// <param name="containerId">The id of the container to retrieve.</param>
        /// <returns>The <see cref="IContainer"/> matching the given id.</returns>
        public IContainer GetContainerById(string containerId);

        /// <summary>Persists a new document in the store.</summary>
        /// <param name="document">The document to create.</param>
        public void CreateDocument(Document document);

        /// <summary>Returns the total number of documents currently stored.</summary>
        /// <returns>The count of stored documents as an unsigned integer.</returns>
        public uint GetAmountOfDocuments();

        /// <summary>Returns the document with the specified id.</summary>
        /// <param name="id">The id of the document to retrieve.</param>
        /// <returns>The <see cref="Document"/> matching the given id.</returns>
        public Document GetDocument(string id);

        /// <summary>Persists a new tag in the store.</summary>
        /// <param name="tag">The tag to create.</param>
        public void CreateTag(Tag tag);

        /// <summary>Assigns an existing tag to an existing document.</summary>
        /// <param name="documentId">The id of the document.</param>
        /// <param name="tagId">The id of the tag to assign.</param>
        public void AssignTag(string documentId, string tagId);

        /// <summary>Removes a tag assignment from an existing document.</summary>
        /// <param name="documentId">The id of the document.</param>
        /// <param name="tagId">The id of the tag to unassign.</param>
        public void UnassignTag(string documentId, string tagId);

        /// <summary>Returns all tags currently stored.</summary>
        /// <returns>An array of <see cref="TagDTO"/> representing every stored tag.</returns>
        public TagDTO[] GetAllTags();

        /// <summary>Returns the ids of all documents currently stored.</summary>
        /// <returns>A set containing every document id.</returns>
        public ISet<string> GetAllDocumentIds();

        /// <summary>Returns the id of the storage location that (transitively) contains the specified containee.</summary>
        /// <param name="containeeId">The id of the containee whose ancestor storage location is sought.</param>
        /// <returns>The id of the enclosing storage location.</returns>
        public string GetIdOfStorageLocationContainedIn(string containeeId);

        /// <summary>Returns whether the specified user is the owner of the specified storage location.</summary>
        /// <param name="userId">The id of the user to check.</param>
        /// <param name="storageLocationId">The id of the storage location to check.</param>
        /// <returns><see langword="true"/> if the user owns the storage location; otherwise <see langword="false"/>.</returns>
        public bool UserIsOwnerOfStorageLocation(string userId, string storageLocationId);

        /// <summary>Returns whether the specified storage location has been shared with the specified user.</summary>
        /// <param name="storageLocationId">The id of the storage location to check.</param>
        /// <param name="userId">The id of the user to check.</param>
        /// <returns><see langword="true"/> if the storage location is shared with the user; otherwise <see langword="false"/>.</returns>
        public bool StorageLocationIsSharedWithUser(string storageLocationId, string userId);

        /// <summary>Creates a new storage location with the given name and returns its generated id.</summary>
        /// <param name="name">The display name of the new storage location.</param>
        /// <returns>The id of the newly created storage location.</returns>
        public string AddStoragLocation(string name);

        /// <summary>Sets the owner of the specified storage location to the specified user.</summary>
        /// <param name="storageLocationId">The id of the storage location.</param>
        /// <param name="userId">The id of the user who should become the owner.</param>
        public void SetOwnerOfStorageLocation(string storageLocationId, string userId);

        /// <summary>Creates a new folder with the given name and returns its generated id.</summary>
        /// <param name="name">The display name of the new folder.</param>
        /// <returns>The id of the newly created folder.</returns>
        public string AddFolder(string name);

        /// <summary>Returns the id of the direct parent container of the specified containee.</summary>
        /// <param name="containeeId">The id of the containee whose parent is sought.</param>
        /// <returns>The id of the parent container.</returns>
        public string GetParentIdOfContainee(string containeeId);

        /// <summary>Returns whether the specified id belongs to a containee (document or folder).</summary>
        /// <param name="id">The id to check.</param>
        /// <returns><see langword="true"/> if the id identifies a containee; otherwise <see langword="false"/>.</returns>
        public bool IsContaineeId(string id);

        /// <summary>Returns whether the specified id belongs to a storage location.</summary>
        /// <param name="id">The id to check.</param>
        /// <returns><see langword="true"/> if the id identifies a storage location; otherwise <see langword="false"/>.</returns>
        public bool IsStorageLocationId(string id);

        /// <summary>Sets the parent container of the specified containee.</summary>
        /// <param name="containee">The containee whose parent should be set.</param>
        /// <param name="parentContainerId">The id of the new parent container.</param>
        public void SetParentOfContainee(IContainee containee, string parentContainerId);

        /// <summary>Returns whether deletion of the specified document is currently permitted.</summary>
        /// <param name="documentId">The id of the document to check.</param>
        /// <returns><see langword="true"/> if deletion is allowed; otherwise <see langword="false"/>.</returns>
        public bool DeleteIsAllowed(string documentId);

        /// <summary>Marks the specified document as soft-deleted without removing it from the store.</summary>
        /// <param name="documentId">The id of the document to soft-delete.</param>
        public void SoftDelete(string documentId);

        /// <summary>Stores the AI-generated summaries of the specified document.</summary>
        /// <param name="documentId">The id of the document whose summaries should be stored.</param>
        /// <param name="shortSummary">The short summary, or <see langword="null"/>.</param>
        /// <param name="longSummary">The long summary, or <see langword="null"/>.</param>
        public void SetAISummary(string documentId, string? shortSummary, string? longSummary);

        /// <summary>Returns the value of the setting with the given key, or <see langword="null"/> if the setting is not set.</summary>
        /// <param name="key">The key of the setting.</param>
        /// <returns>The stored value, or <see langword="null"/> if no value is stored for the key.</returns>
        public string? GetSetting(string key);

        /// <summary>Stores (inserts or updates) the value of the setting with the given key.</summary>
        /// <param name="key">The key of the setting.</param>
        /// <param name="value">The value to store.</param>
        public void SetSetting(string key, string value);

        /// <summary>Records that <paramref name="newDocumentId"/> is a new version of <paramref name="oldDocumentId"/>. Both documents keep existing unchanged; only the version-relationship is stored.</summary>
        /// <param name="oldDocumentId">The id of the predecessor-document which is superseded by the new version.</param>
        /// <param name="newDocumentId">The id of the new version.</param>
        public void AddDocumentVersionLink(string oldDocumentId, string newDocumentId);

        /// <summary>Returns the ids of all documents which have been superseded by a newer version.</summary>
        /// <returns>The set of superseded document-ids.</returns>
        public ISet<string> GetSupersededDocumentIds();

        /// <summary>Returns the id of the document which is the direct predecessor (previous version) of the given document, or <see langword="null"/> if there is none.</summary>
        /// <param name="documentId">The id of the document whose predecessor is requested.</param>
        /// <returns>The predecessor-document-id, or <see langword="null"/>.</returns>
        public string? GetPreviousVersionId(string documentId);

        /// <summary>Returns the id of the document which is the direct successor (next version) of the given document, or <see langword="null"/> if there is none.</summary>
        /// <param name="documentId">The id of the document whose successor is requested.</param>
        /// <returns>The successor-document-id, or <see langword="null"/>.</returns>
        public string? GetNextVersionId(string documentId);

        /// <summary>Permanently removes the specified container or containee and all associated data from the store.</summary>
        /// <param name="containerOrContaineeId">The id of the container or containee to delete.</param>
        public void HardDelete(string containerOrContaineeId);

        /// <summary>Grants the specified user permission to view the specified storage location.</summary>
        /// <param name="storageLocationId">The id of the storage location to share.</param>
        /// <param name="sharedWithUserId">The id of the user to grant access to.</param>
        public void AuthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId);

        /// <summary>Revokes the specified user's permission to view the specified storage location.</summary>
        /// <param name="storageLocationId">The id of the storage location.</param>
        /// <param name="sharedWithUserId">The id of the user whose access should be revoked.</param>
        public void UnauthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId);

        /// <summary>Renames the specified container to the given new name.</summary>
        /// <param name="containerId">The id of the container to rename.</param>
        /// <param name="newName">The new name to assign.</param>
        public void Rename(string containerId, string newName);

        /// <summary>Updates the specified document on behalf of the requesting user.</summary>
        /// <param name="requesterUserId">The id of the user performing the update.</param>
        /// <param name="updatedDocument">The document object containing the updated values.</param>
        public void Update(string requesterUserId, Document updatedDocument);

        /// <summary>Returns a preview representation of the document with the specified id.</summary>
        /// <param name="id">The id of the document whose preview is requested.</param>
        /// <returns>A <see cref="DocumentPreview"/> for the specified document.</returns>
        public DocumentPreview GetDocumentPreview(string id);

        /// <summary>Returns the ids of all storage locations currently stored.</summary>
        /// <returns>An enumerable of every storage location id.</returns>
        public IEnumerable<string> GetAllStorageLocationIds();

        /// <summary>Returns the storage location with the specified id.</summary>
        /// <param name="storageLocationId">The id of the storage location to retrieve.</param>
        /// <returns>The <see cref="StorageLocation"/> matching the given id.</returns>
        public StorageLocation GetStorageLocation(string storageLocationId);

        /// <summary>Returns the folder with the specified id.</summary>
        /// <param name="folderId">The id of the folder to retrieve.</param>
        /// <returns>The <see cref="Folder"/> matching the given id.</returns>
        public Folder GetFolder(string folderId);

        /// <summary>Returns whether the specified id belongs to a storage location.</summary>
        /// <param name="contentId">The id to check.</param>
        /// <returns><see langword="true"/> if the id identifies a storage location; otherwise <see langword="false"/>.</returns>
        public bool IsStorageLocation(string contentId);

        /// <summary>Returns whether the specified id belongs to a folder.</summary>
        /// <param name="contentId">The id to check.</param>
        /// <returns><see langword="true"/> if the id identifies a folder; otherwise <see langword="false"/>.</returns>
        public bool IsFolder(string contentId);

        /// <summary>Returns whether the specified id belongs to a document.</summary>
        /// <param name="contentId">The id to check.</param>
        /// <returns><see langword="true"/> if the id identifies a document; otherwise <see langword="false"/>.</returns>
        public bool IsDocument(string contentId);

        /// <summary>Returns the highest readable id that has been issued so far.</summary>
        /// <returns>The latest readable id as an unsigned 64-bit integer.</returns>
        public ulong GetLatestReadableId();

        /// <summary>Removes the specified child from the specified parent container.</summary>
        /// <param name="parentId">The id of the parent container.</param>
        /// <param name="childId">The id of the child to remove.</param>
        public void RemoveChild(string parentId, string childId);

        /// <summary>Returns the internal id that corresponds to the given human-readable id.</summary>
        /// <param name="readableId">The human-readable numeric id to look up.</param>
        /// <returns>The internal string id matching the readable id.</returns>
        public string GetIdFromReadableId(uint readableId);

        /// <summary>
        /// This function searches for documents which are available for the given user and matches at least one search-term in any kind.
        /// This function is supposed to order the results descending regarding to relevance.
        /// The result-list does not contain duplicated items.
        /// </summary>
        /// <returns>
        /// Returns an ordered list of document-ids which are available for the given user and match at least one search-term in any kind.
        /// The order is descending regarding to relevance.
        /// </returns>
        public IList<string> Search(string searchTerm);

        /// <summary>Returns the ids of all documents that must be hard-deleted immediately.</summary>
        /// <returns>An enumerable of document ids that are due for hard deletion.</returns>
        public IEnumerable<string> GetIdsOfDocumentsWhichMustBeHardDeletedNow();

        /// <summary>Resets the persistence store to its initial empty state.</summary>
        public void Reset();

        /// <summary>
        /// Returns the user that is linked to the given external OIDC provider and subject.
        /// Returns <see langword="null"/> if no such user exists.
        /// </summary>
        public Model.BusinessTypes.User? GetUserByExternalLogin(string providerId, string subject);

        /// <summary>Returns whether a user with the given external login exists.</summary>
        public bool UserWithExternalLoginExists(string providerId, string subject);
    }
}
