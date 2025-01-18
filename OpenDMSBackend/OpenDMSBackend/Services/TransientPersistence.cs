using System;
using System.Collections.Generic;
using System.Linq;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;

namespace OpenDMSBackend.Core.Services
{
    public sealed class TransientPersistence : IPersistence
    {
        private readonly IDictionary<string/*id*/, StorageLocation> _StorageLocations;
        private readonly IDictionary<string/*id*/, Folder> _Folders;
        private readonly IDictionary<string/*id*/, Document> _Documents;
        private readonly IDictionary<string/*id*/, Tag> _Tags;
        private readonly IDictionary<string/*containee-id*/, string/*container-id*/> _ContaineeContainerAssignments;
        private readonly IDictionary<string/*storagelocation-id*/, string/*user-id*/> _StorageLocationOwnerAssignments;
        private readonly IIdGenerator<ulong> _IdGenerator;
        private readonly IAuthenticationServicePersistence<User> _TransientAuthenticationServicePersistence;

        public TransientPersistence(IAuthenticationServicePersistence<User> transientAuthenticationServicePersistence,IIdGenerator<ulong> idGenerator)
        {
            this._TransientAuthenticationServicePersistence = transientAuthenticationServicePersistence;
            this._StorageLocations = new Dictionary<string, StorageLocation>();
            this._Folders = new Dictionary<string, Folder>();
            this._Documents = new Dictionary<string, Document>();
            this._ContaineeContainerAssignments = new Dictionary<string, string>();
            this._StorageLocationOwnerAssignments = new Dictionary<string, string>();
            this._Tags = new Dictionary<string, Tag>();
            this._IdGenerator = idGenerator;
            this.Initialize();
        }

        private void Initialize()
        {
            this.Reset();
        }

        public void Reset()
        {
            this._StorageLocationOwnerAssignments.Clear();
            this._StorageLocations.Clear();
            this._Folders.Clear();
            this._Documents.Clear();
            this._ContaineeContainerAssignments.Clear();
            this._StorageLocationOwnerAssignments.Clear();
            this._Tags.Clear();
            this._IdGenerator.Reset();
        }

        public void CreateDocument(Document document)
        {
            this._Documents[document.Id] = document;
        }

        public bool DocumentExists(string id)
        {
            return this._Documents.ContainsKey(id);
        }

        public bool IsAvailable()
        {
            return true;
        }

        public void Dispose()
        {
            Utilities.NoOperation();
        }

        public uint GetAmountOfDocuments()
        {
            return (uint)this._Documents.Count;
        }

        public bool UserWithNameExists(string username)
        {
            return this._TransientAuthenticationServicePersistence.UserWithNameExists(username);
        }

        public bool UserWithIdExists(string userId)
        {
            return this._TransientAuthenticationServicePersistence.UserWithIdExists(userId);
        }

        public ulong GetNewReadableId()
        {
            return this._IdGenerator.GenerateNewId();
        }

        public Document GetDocument(string id)
        {
            var result = this._Documents[id];
            return result;
        }

        public void CreateTag(Tag tag)
        {
            this._Tags[tag.Id] = tag;
        }

        private Tag GetTag(string id)
        {
            return this._Tags[id];
        }

        public void AssignTag(string documentId, string tagId)
        {
            this.GetDocument(documentId).Tags.Add(this.GetTag(tagId));
        }

        public void UnassignTag(string documentId, string tagId)
        {
            this.GetDocument(documentId).Tags.Remove(this.GetTag(tagId));
        }

        public TagDTO[] GetAllTags()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAllDocumentIds()
        {
            return this._Documents.Keys;
        }

        public string GetIdOfStorageLocationContainedIn(string id)
        {
            if (this.IsContaineeId(id))
            {
                return this.GetIdOfStorageLocationContainedIn(this.GetParentIdOfContainee(id));
            }
            else if (this.IsStorageLocationId(id))
            {
                return id;
            }
            else
            {
                throw new KeyNotFoundException($"Id {id} not found.");
            }
        }

        public bool UserIsOwnerOfStorageLocation(string userId, string storageLocationId)
        {
            if (this._StorageLocations.ContainsKey(storageLocationId))
            {
                return this._StorageLocationOwnerAssignments[storageLocationId] == userId;
            }
            else
            {
                return false;
            }
        }

        public bool StorageLocationIsSharedWithUser(string storageLocationId, string userId)
        {
            return false;//TODO
        }

        public string AddStoragLocation(string name)
        {
            var sl = new StorageLocation();
            sl.Id = Guid.NewGuid().ToString();
            sl.Name = name;
            this._StorageLocations[sl.Id] = sl;
            return sl.Id;
        }

        public void SetOwnerOfStorageLocation(string storageLocationId, string userId)
        {
            this._StorageLocationOwnerAssignments[storageLocationId] = userId;
        }

        public string AddFolder(string name)
        {
            var folder = new Folder()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
            };
            this._Folders[folder.Id] = folder;
            return folder.Id;
        }

        public void SetParentOfContainee(IContainee containee, string parentContainerId)
        {
            this._ContaineeContainerAssignments[containee.Id] = parentContainerId;
            OpenDMSBackend.Core.Miscellaneous.Utilities.DoForContentObject(this, parentContainerId,
            (storageId) =>
            {
                this._StorageLocations[parentContainerId].Content.Add(containee);
            },
            (folderId) =>
            {
                this._Folders[parentContainerId].Content.Add(containee);
            },
            (documentId) =>
            {
                GRYLibrary.Core.Misc.Utilities.NoOperation();
            });
        }

        public void Delete(string containerOrContaineeId)
        {
            OpenDMSBackend.Core.Miscellaneous.Utilities.DoForContentObject(this, containerOrContaineeId,
                (storageId) =>
                {
                    this._StorageLocationOwnerAssignments.Remove(storageId);
                    //TODO remove all related stuff from _ContaineeContainerAssignments
                    this._StorageLocations.Remove(storageId);
                },
                (folderId) =>
                {
                    //TODO remove all related stuff from _ContaineeContainerAssignments
                    this._Folders.Remove(folderId);
                },
                (documentId) =>
                {
                    //TODO remove all related stuff from _ContaineeContainerAssignments
                    this._Documents.Remove(documentId);
                });
        }

        public void AuthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId)
        {
            throw new NotImplementedException();
        }

        public void UnauthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId)
        {
            throw new NotImplementedException();
        }

        public void Rename(string containerId, string newName)
        {
            throw new NotImplementedException();
        }

        public void Update(string requesterUserId, Document updatedDocument)
        {
            throw new NotImplementedException();
        }

        public IContainee GetContaineeById(string containeeId)
        {
            if (this._Documents.TryGetValue(containeeId, out Document? document))
            {
                return document;
            }
            if (this._Folders.TryGetValue(containeeId, out Folder? folder))
            {
                return folder;
            }
            throw new KeyNotFoundException($"No {nameof(IContainee)} available with id \"{containeeId}\".");
        }

        public string GetParentIdOfContainee(string containeeId)
        {
            return this._ContaineeContainerAssignments[containeeId];
        }

        public bool IsContaineeId(string containeeId)
        {
            return this._Documents.ContainsKey(containeeId) || this._Folders.ContainsKey(containeeId);
        }

        public bool IsStorageLocationId(string id)
        {
            return this._StorageLocations.ContainsKey(id);
        }

        public DocumentPreview GetDocumentPreview(string id)
        {
            return this.GetDocument(id).GetPreview();
        }

        public IEnumerable<string> GetAllStorageLocationIds()
        {
            return this._StorageLocations.Keys.ToList();
        }

        public StorageLocation GetStorageLocation(string storageLocationId)
        {
            return this._StorageLocations[storageLocationId];
        }

        public Folder GetFolder(string folderId)
        {
            return this._Folders[folderId];
        }

        public bool IsStorageLocation(string contentId)
        {
            return this._StorageLocations.ContainsKey(contentId);
        }

        public bool IsFolder(string contentId)
        {
            return this._Folders.ContainsKey(contentId);
        }

        public bool IsDocument(string contentId)
        {
            return this._Documents.ContainsKey(contentId);
        }

        public ulong GetLatestReadableId()
        {
            return this.GetAmountOfDocuments();
        }
    }
}