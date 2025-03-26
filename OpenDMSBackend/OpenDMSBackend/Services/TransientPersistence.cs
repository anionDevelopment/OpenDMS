using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
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

        public TransientPersistence(IAuthenticationServicePersistence<User> transientAuthenticationServicePersistence, IIdGenerator<ulong> idGenerator)
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
            Miscellaneous.Utilities.DoForContentObject(this, parentContainerId,
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
                Utilities.NoOperation();
            });
        }

        public void Delete(string containerOrContaineeId)
        {
            Miscellaneous.Utilities.DoForContentObject(this, containerOrContaineeId,
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

        public IContainer GetContainerById(string containerId)
        {
            if (this._StorageLocations.TryGetValue(containerId, out StorageLocation? storageLocation))
            {
                return storageLocation;
            }
            if (this._Folders.TryGetValue(containerId, out Folder? folder))
            {
                return folder;
            }
            throw new KeyNotFoundException($"No {nameof(IContainer)} available with id \"{containerId}\".");
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

        public IDictionary<string, User> GetAllUsers()
        {
            return this._TransientAuthenticationServicePersistence.GetAllUsers();
        }

        public ISet<GRYLibrary.Core.APIServer.CommonDBTypes.Role> GetAllRoles()
        {
            return this._TransientAuthenticationServicePersistence.GetAllRoles();
        }

        public void AddRole(GRYLibrary.Core.APIServer.CommonDBTypes.Role role)
        {
            this._TransientAuthenticationServicePersistence.AddRole(role);
        }

        public void UpdateRole(GRYLibrary.Core.APIServer.CommonDBTypes.Role role)
        {
            this._TransientAuthenticationServicePersistence.UpdateRole(role);
        }

        public void DeleteRoleByName(string roleName)
        {
            this._TransientAuthenticationServicePersistence.DeleteRoleByName(roleName);
        }

        public bool AccessTokenExists(string accessToken, out User? user)
        {
            return this._TransientAuthenticationServicePersistence.AccessTokenExists(accessToken, out user);
        }

        public void AddUser(User newUser)
        {
            this._TransientAuthenticationServicePersistence.AddUser(newUser);
        }

        public User GetUserById(string userId)
        {
            return this._TransientAuthenticationServicePersistence.GetUserById(userId);
        }

        public User GetUserByName(string userName)
        {
            return this._TransientAuthenticationServicePersistence.GetUserByName(userName);
        }

        public void RemoveUser(string userId)
        {
            this._TransientAuthenticationServicePersistence.RemoveUser(userId);
        }

        public bool RoleExists(string roleName)
        {
            return this._TransientAuthenticationServicePersistence.RoleExists(roleName);
        }

        public void AddRoleToUser(string userId, string roleId)
        {

            this._TransientAuthenticationServicePersistence.AddRoleToUser(userId, roleId);
        }

        public void RemoveRoleFromUser(string userId, string roleId)
        {
            this._TransientAuthenticationServicePersistence.RemoveRoleFromUser(userId, roleId);
        }

        public bool UserHasRole(string userId, string roleId)
        {
            return this._TransientAuthenticationServicePersistence.UserHasRole(userId, roleId);
        }

        public User GetUserByAccessToken(string accessToken)
        {
            return this._TransientAuthenticationServicePersistence.GetUserByAccessToken(accessToken);
        }

        public void UpdateUser(User user)
        {
            this._TransientAuthenticationServicePersistence.UpdateUser(user);
        }

        public AccessToken GetAccessToken(string accessToken)
        {
            return this._TransientAuthenticationServicePersistence.GetAccessToken(accessToken);
        }

        public void AddAccessToken(string userId, AccessToken newAccessToken)
        {
            this._TransientAuthenticationServicePersistence.AddAccessToken(userId, newAccessToken);
        }

        public void RemoveAccessToken(string accessToken)
        {
            this._TransientAuthenticationServicePersistence.RemoveAccessToken(accessToken);
        }

        public ISet<AccessToken> GetAllAccessTokenOfUser(string userId)
        {
            return this._TransientAuthenticationServicePersistence.GetAllAccessTokenOfUser(userId);
        }

        public void RemoveChild(string parentId, string childId)
        {
            IContainer container = this.GetContainerById(parentId);
            container.Content = container.Content.Where(child => child.Id != childId).ToHashSet();
        }

        public string GetIdFromReadableId(uint readableId)
        {
            foreach (var document in this._Documents)
            {
                if (document.Value.ReadableId == readableId)
                {
                    return document.Value.Id;
                }
            }
            throw new KeyNotFoundException($"No document found with readable id '{readableId}'.");
        }

        public IList<DocumentPreview> Search(string requesterUserId, string[] searchTerms)
        {
            IList<DocumentPreview> result = new List<DocumentPreview>();
            foreach (var document in this._Documents.Values)
            {
                foreach (var searchTerm in searchTerms)
                {
                    if (this.DocumentMatchesSearch(document, requesterUserId, searchTerm))
                    {
                        result.Add(document.GetPreview());
                        break;
                    }
                }
            }
            return result;
        }

        private bool DocumentMatchesSearch(Document document, string requesterUserId, string searchTerm)
        {
            if (document.Title.Value.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            if (document.Filename.Value.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            if (document.OriginalFilename.Value.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            if (document.OCRContent.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            foreach (var tag in document.Tags)
            {
                if (tag.Name.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}