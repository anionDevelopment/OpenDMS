using GRYLibrary.Core.APIServer.Services;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using System;
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
        public string GetStorageLocationId(string containeeId);
        public bool UserIsOwnerOfStorageLocation(string userId, string storageLocationId);
        public bool StorageLocationIsSharedWithUser(string storageLocationId, string userId);
        public string AddStoragLocation(string name);
        public void SetOwnerOfStorageLocation(string storageLocationId, string userId);
        public string AddFolder(string name);
        public string GetParentIdOfContainee(string containeeId);
        public bool IsContaineeId(string id);
        public bool IsStorageLocationId(string id);
        void SetParentOfContainee(string containeeId, string parentContainerId);
        void Delete(string containerOrContaineeId);
        void AuthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId);
        void UnauthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId);
        void Rename(string containerId, string newName);
        void Update(string requesterUserId, Document updatedDocument);
        DocumentPreview GetDocumentPreview(string id);
    }
}
