using GRYLibrary.Core.APIServer.Services;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Services
{
    public interface IPersistence : IExternalService
    {
        public IContainee GetContaineeById(string containeeId);
        public void CreateDocument(Document document);
        public bool DocumentExists(string id);
        public uint GetAmountOfDocuments();
        public void Reset();
        public bool UserWithNameExists(string username);
        public bool UserWithIdExists(string userId);
        public ulong GetNewReadableId();
        public Document GetDocument(string id);
        public void CreateTag(Tag tag);
        public void AssignTag(string documentId, string tagId);
        public void UnassignTag(string documentId, string tagId);
        public TagDTO[] GetAllTags();
        public IEnumerable<string> GetAllDocumentIds();
        public string GetIdOfStorageLocationContainedIn(string containeeId);
        public bool UserIsOwnerOfStorageLocation(string userId, string storageLocationId);
        public bool StorageLocationIsSharedWithUser(string storageLocationId, string userId);
        public string AddStoragLocation(string name);
        public void SetOwnerOfStorageLocation(string storageLocationId, string userId);
        public string AddFolder(string name);
        public string GetParentIdOfContainee(string containeeId);
        public bool IsContaineeId(string id);
        public bool IsStorageLocationId(string id);
        public void SetParentOfContainee(IContainee containee, string parentContainerId);
        public void Delete(string containerOrContaineeId);
        public void AuthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId);
        public void UnauthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId);
        public void Rename(string containerId, string newName);
        public void Update(string requesterUserId, Document updatedDocument);
        public DocumentPreview GetDocumentPreview(string id);
        public IEnumerable<string> GetAllStorageLocationIds();
        public StorageLocation GetStorageLocation(string storageLocationId);
        public Folder GetFolder(string folderId);
        public bool IsStorageLocation(string contentId);
        public bool IsFolder(string contentId);
        public bool IsDocument(string contentId);
    }
}
