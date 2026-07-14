using System;
using System.Collections.Generic;
using System.Linq;
using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
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
        private readonly IDictionary<string/*storagelocation-id*/, ISet<string>/*user-ids*/> _StorageLocationOwnerAssignments;
        private readonly IDictionary<string/*storagelocation-id*/, ISet<string>/*user-ids*/> _StorageLocationViewGrants;
        private readonly IDictionary<string/*storagelocation-id*/, ISet<string>/*user-ids*/> _StorageLocationEditGrants;
        private readonly IDictionary<string/*key*/, string/*value*/> _Settings;
        private readonly IDictionary<string/*field-definition-id*/, MetadataFieldDefinition> _MetadataFieldDefinitions;
        private readonly IList<DocumentVersionEntry> _DocumentVersions;
        private readonly IIdGenerator<ulong> _IdGenerator;
        private readonly ITimeService _TimeService;
        private readonly IAuthenticationServicePersistence<User> _TransientAuthenticationServicePersistence;
        private bool _LogConnectionErrors = true;
        private static readonly object _Lock = new object();
        /// <summary>Initializes a new in-memory persistence instance.</summary>
        /// <param name="transientAuthenticationServicePersistence">The in-memory authentication persistence delegate.</param>
        /// <param name="idGenerator">Generator for unique numeric readable ids.</param>
        /// <param name="timeService">The service used to obtain the current time.</param>
        public TransientPersistence(IAuthenticationServicePersistence<User> transientAuthenticationServicePersistence, IIdGenerator<ulong> idGenerator, ITimeService timeService)
        {
            this._TransientAuthenticationServicePersistence = transientAuthenticationServicePersistence;
            this._StorageLocations = new Dictionary<string, StorageLocation>();
            this._Folders = new Dictionary<string, Folder>();
            this._Documents = new Dictionary<string, Document>();
            this._ContaineeContainerAssignments = new Dictionary<string, string>();
            this._StorageLocationOwnerAssignments = new Dictionary<string, ISet<string>>();
            this._StorageLocationViewGrants = new Dictionary<string, ISet<string>>();
            this._StorageLocationEditGrants = new Dictionary<string, ISet<string>>();
            this._Settings = new Dictionary<string, string>();
            this._MetadataFieldDefinitions = new Dictionary<string, MetadataFieldDefinition>();
            this._DocumentVersions = new List<DocumentVersionEntry>();
            this._Tags = new Dictionary<string, Tag>();
            this._IdGenerator = idGenerator;
            this._TimeService = timeService;
            this.Initialize();
        }

        private void Initialize()
        {
            this.Reset();
        }

        /// <inheritdoc />
        public void Reset()
        {
            this._StorageLocationOwnerAssignments.Clear();
            this._StorageLocations.Clear();
            this._Folders.Clear();
            this._Documents.Clear();
            this._ContaineeContainerAssignments.Clear();
            this._StorageLocationOwnerAssignments.Clear();
            this._StorageLocationViewGrants.Clear();
            this._StorageLocationEditGrants.Clear();
            this._Settings.Clear();
            this._MetadataFieldDefinitions.Clear();
            this._DocumentVersions.Clear();
            this._Tags.Clear();
            this._IdGenerator.Reset();
        }

        /// <inheritdoc />
        public void CreateDocument(Document document)
        {
            this._Documents[document.Id] = document;
        }

        /// <inheritdoc />
        public (bool, Exception?) IsAvailable()
        {
            return (true, null);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Utilities.NoOperation();
        }

        /// <inheritdoc />
        public uint GetAmountOfDocuments()
        {
            return (uint)this._Documents.Count;
        }

        /// <inheritdoc />
        public bool UserWithNameExists(string username)
        {
            return this._TransientAuthenticationServicePersistence.UserWithNameExists(username);
        }

        /// <inheritdoc />
        public bool UserWithIdExists(string userId)
        {
            return this._TransientAuthenticationServicePersistence.UserWithIdExists(userId);
        }

        /// <summary>Generates and returns a new unique readable id.</summary>
        /// <returns>The next available unique readable id.</returns>
        public ulong GetNewReadableId()
        {
            return this._IdGenerator.GenerateNewId();
        }

        /// <inheritdoc />
        public Document GetDocument(string id)
        {
            Document result = this._Documents[id];
            return result;
        }

        /// <inheritdoc />
        public void CreateTag(Tag tag)
        {
            this._Tags[tag.Id] = tag;
        }

        private Tag GetTag(string id)
        {
            return this._Tags[id];
        }

        /// <inheritdoc />
        public void AssignTag(string documentId, string tagId)
        {
            this.GetDocument(documentId).Tags.Add(this.GetTag(tagId));
        }

        /// <inheritdoc />
        public void UnassignTag(string documentId, string tagId)
        {
            this.GetDocument(documentId).Tags.Remove(this.GetTag(tagId));
        }

        /// <inheritdoc />
        public TagDTO[] GetAllTags()
        {
            return this._Tags.Values.Select(t => t.ToDTO()).ToArray();
        }

        /// <inheritdoc />
        public ISet<string> GetAllDocumentIds()
        {
            return this._Documents.Keys.ToHashSet();
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool UserIsOwnerOfStorageLocation(string userId, string storageLocationId)
        {
            //the "owner"-concept is a moderator of a content-object (storage-location, folder or document); a content-object can have several moderators.
            return this.HasGrant(this._StorageLocationOwnerAssignments, storageLocationId, userId);
        }

        /// <inheritdoc />
        public ISet<string> GetOwnersOfStorageLocation(string storageLocationId)
        {
            return this._StorageLocationOwnerAssignments.TryGetValue(storageLocationId, out ISet<string>? owners) ? new HashSet<string>(owners) : new HashSet<string>();
        }

        /// <inheritdoc />
        public void RemoveOwnerOfStorageLocation(string storageLocationId, string userId)
        {
            this.RemoveGrant(this._StorageLocationOwnerAssignments, storageLocationId, userId);
        }

        /// <inheritdoc />
        public bool StorageLocationIsSharedWithUser(string storageLocationId, string userId)
        {
            //an edit-grant implies a view-grant.
            return this.HasGrant(this._StorageLocationViewGrants, storageLocationId, userId) || this.HasGrant(this._StorageLocationEditGrants, storageLocationId, userId);
        }

        /// <inheritdoc />
        public bool StorageLocationIsEditableByUser(string storageLocationId, string userId)
        {
            return this.HasGrant(this._StorageLocationEditGrants, storageLocationId, userId);
        }

        private bool HasGrant(IDictionary<string, ISet<string>> grants, string storageLocationId, string userId)
        {
            return grants.TryGetValue(storageLocationId, out ISet<string>? userIds) && userIds.Contains(userId);
        }

        private void AddGrant(IDictionary<string, ISet<string>> grants, string storageLocationId, string userId)
        {
            if (!grants.TryGetValue(storageLocationId, out ISet<string>? userIds))
            {
                userIds = new HashSet<string>();
                grants[storageLocationId] = userIds;
            }
            userIds.Add(userId);
        }

        private void RemoveGrant(IDictionary<string, ISet<string>> grants, string storageLocationId, string userId)
        {
            if (grants.TryGetValue(storageLocationId, out ISet<string>? userIds))
            {
                userIds.Remove(userId);
            }
        }

        /// <inheritdoc />
        public string AddStoragLocation(string name)
        {
            StorageLocation sl = new StorageLocation();
            sl.Id = Guid.NewGuid().ToString();
            sl.Name = name;
            this._StorageLocations[sl.Id] = sl;
            return sl.Id;
        }

        /// <inheritdoc />
        public void SetOwnerOfStorageLocation(string storageLocationId, string userId)
        {
            //adds the user as a moderator ("owner") of the content-object; a content-object can have several moderators.
            this.AddGrant(this._StorageLocationOwnerAssignments, storageLocationId, userId);
        }

        /// <inheritdoc />
        public string AddFolder(string name)
        {
            Folder folder = new Folder()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
            };
            this._Folders[folder.Id] = folder;
            return folder.Id;
        }

        /// <inheritdoc />
        public void SetParentOfContainee(IContainee containee, string parentContainerId)
        {
            this._ContaineeContainerAssignments[containee.Id] = parentContainerId;
            Core.Misc.Utilities.DoForContentObject(this, parentContainerId,
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

        /// <inheritdoc />
        public void HardDelete(string containerOrContaineeId)
        {
            Core.Misc.Utilities.DoForContentObject(this, containerOrContaineeId,
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
                    //hard-deleting a document does not remove its row: the binary-content and preview are cleared, the OCR-content and AI-summaries are cleared, the tags are unassigned and it is marked as hard-deleted. The row and version-entry are kept for traceability and no new version is created.
                    Document document = this._Documents[documentId];
                    document.Content = System.Array.Empty<byte>();
                    document.Preview = System.Array.Empty<byte>();
                    document.OCRContent = string.Empty;
                    document.AISummaryShort = null;
                    document.AISummaryLong = null;
                    document.Tags.Clear();
                    document.IsHardDeleted = true;
                });
        }

        /// <inheritdoc />
        public void AuthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId)
        {
            this.AddGrant(this._StorageLocationViewGrants, storageLocationId, sharedWithUserId);
        }

        /// <inheritdoc />
        public void UnauthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId)
        {
            //revoking the view-permission also revokes the (stronger) edit-permission.
            this.RemoveGrant(this._StorageLocationViewGrants, storageLocationId, sharedWithUserId);
            this.RemoveGrant(this._StorageLocationEditGrants, storageLocationId, sharedWithUserId);
        }

        /// <inheritdoc />
        public void AuthorizeUserToEditStorageLocation(string storageLocationId, string editUserId)
        {
            //an edit-grant implies a view-grant; both are stored so that the view-check succeeds too.
            this.AddGrant(this._StorageLocationViewGrants, storageLocationId, editUserId);
            this.AddGrant(this._StorageLocationEditGrants, storageLocationId, editUserId);
        }

        /// <inheritdoc />
        public void UnauthorizeUserToEditStorageLocation(string storageLocationId, string editUserId)
        {
            //revoking the edit-permission keeps the view-permission.
            this.RemoveGrant(this._StorageLocationEditGrants, storageLocationId, editUserId);
        }

        /// <inheritdoc />
        public void Rename(string containerId, string newName)
        {
           GetContainerById(containerId).Name = newName;
        }

        /// <inheritdoc />
        public void Update(string requesterUserId, Document updatedDocument)
        {
            _Documents[updatedDocument.Id] = updatedDocument;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public string GetParentIdOfContainee(string containeeId)
        {
            return this._ContaineeContainerAssignments[containeeId];
        }

        /// <inheritdoc />
        public bool IsContaineeId(string containeeId)
        {
            return this._Documents.ContainsKey(containeeId) || this._Folders.ContainsKey(containeeId);
        }

        /// <inheritdoc />
        public bool IsStorageLocationId(string id)
        {
            return this._StorageLocations.ContainsKey(id);
        }

        /// <inheritdoc />
        public DocumentPreview GetDocumentPreview(string id)
        {
            return this.GetDocument(id).GetPreview();
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAllStorageLocationIds()
        {
            return this._StorageLocations.Keys.ToList();
        }

        /// <inheritdoc />
        public StorageLocation GetStorageLocation(string storageLocationId)
        {
            return this._StorageLocations[storageLocationId];
        }

        /// <inheritdoc />
        public Folder GetFolder(string folderId)
        {
            return this._Folders[folderId];
        }

        /// <inheritdoc />
        public bool IsStorageLocation(string contentId)
        {
            return this._StorageLocations.ContainsKey(contentId);
        }

        /// <inheritdoc />
        public bool IsFolder(string contentId)
        {
            return this._Folders.ContainsKey(contentId);
        }

        /// <inheritdoc />
        public bool IsDocument(string contentId)
        {
            return this._Documents.ContainsKey(contentId);
        }

        /// <inheritdoc />
        public ulong GetLatestReadableId()
        {
            return this.GetAmountOfDocuments();
        }

        /// <inheritdoc />
        public IDictionary<string, User> GetAllUsers()
        {
            return this._TransientAuthenticationServicePersistence.GetAllUsers();
        }

        /// <inheritdoc />
        public ISet<GRYLibrary.Core.APIServer.CommonDBTypes.Role> GetAllRoles()
        {
            return this._TransientAuthenticationServicePersistence.GetAllRoles();
        }

        /// <inheritdoc />
        public void AddRole(GRYLibrary.Core.APIServer.CommonDBTypes.Role role)
        {
            this._TransientAuthenticationServicePersistence.AddRole(role);
        }

        /// <inheritdoc />
        public void UpdateRole(GRYLibrary.Core.APIServer.CommonDBTypes.Role role)
        {
            this._TransientAuthenticationServicePersistence.UpdateRole(role);
        }

        /// <inheritdoc />
        public void DeleteRoleByName(string roleName)
        {
            this._TransientAuthenticationServicePersistence.DeleteRoleByName(roleName);
        }

        /// <inheritdoc />
        public bool AccessTokenExists(string accessToken, out User? user)
        {
            return this._TransientAuthenticationServicePersistence.AccessTokenExists(accessToken, out user);
        }

        /// <inheritdoc />
        public void AddUser(User newUser)
        {
            this._TransientAuthenticationServicePersistence.AddUser(newUser);
        }

        /// <inheritdoc />
        public User GetUserById(string userId)
        {
            return this._TransientAuthenticationServicePersistence.GetUserById(userId);
        }

        /// <inheritdoc />
        public User GetUserByName(string userName)
        {
            return this._TransientAuthenticationServicePersistence.GetUserByName(userName);
        }

        /// <inheritdoc />
        public void RemoveUser(string userId)
        {
            this._TransientAuthenticationServicePersistence.RemoveUser(userId);
        }

        /// <inheritdoc />
        public bool RoleExists(string roleName)
        {
            return this._TransientAuthenticationServicePersistence.RoleExists(roleName);
        }

        /// <inheritdoc />
        public void AddRoleToUser(string userId, string roleId)
        {

            this._TransientAuthenticationServicePersistence.AddRoleToUser(userId, roleId);
        }

        /// <inheritdoc />
        public void RemoveRoleFromUser(string userId, string roleId)
        {
            this._TransientAuthenticationServicePersistence.RemoveRoleFromUser(userId, roleId);
        }

        /// <inheritdoc />
        public bool UserHasRole(string userId, string roleId)
        {
            return this._TransientAuthenticationServicePersistence.UserHasRole(userId, roleId);
        }

        /// <inheritdoc />
        public User GetUserByAccessToken(string accessToken)
        {
            return this._TransientAuthenticationServicePersistence.GetUserByAccessToken(accessToken);
        }

        /// <inheritdoc />
        public void UpdateUser(User user)
        {
            this._TransientAuthenticationServicePersistence.UpdateUser(user);
        }

        /// <inheritdoc />
        public AccessToken GetAccessToken(string accessToken)
        {
            return this._TransientAuthenticationServicePersistence.GetAccessToken(accessToken);
        }

        /// <inheritdoc />
        public void RemoveAccessToken(string accessToken)
        {
            this._TransientAuthenticationServicePersistence.RemoveAccessToken(accessToken);
        }

        /// <inheritdoc />
        public ISet<AccessToken> GetAllAccessTokenOfUser(string userId)
        {
            return this._TransientAuthenticationServicePersistence.GetAllAccessTokenOfUser(userId);
        }

        /// <inheritdoc />
        public void RemoveChild(string parentId, string childId)
        {
            IContainer container = this.GetContainerById(parentId);
            container.Content = container.Content.Where(child => child.Id != childId).ToHashSet();
        }

        /// <inheritdoc />
        public string GetIdFromReadableId(uint readableId)
        {
            foreach (KeyValuePair<string, Document> document in this._Documents)
            {
                if (document.Value.ReadableId == readableId)
                {
                    return document.Value.Id;
                }
            }
            throw new KeyNotFoundException($"No document found with readable id '{readableId}'.");
        }

        /// <inheritdoc />
        public IList<string> Search(string searchTerm)
        {
            IDictionary<string, uint> result = new Dictionary<string, uint>();
            foreach (Document document in this._Documents.Values)
            {
                uint score = this.DocumentMatchesSearch(document, searchTerm);
                if (0 < score)
                {
                    result[document.Id] = score;
                }
            }
            return result.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        }

        private uint DocumentMatchesSearch(Document document, string searchTerm)
        {
            if (document.Title.Value.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
            {
                return 5;
            }
            if (document.Filename.Value.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
            {
                return 4;
            }
            foreach (Tag tag in document.Tags)
            {
                if (tag.Name.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
                {
                    return 3;
                }
            }
            if (document.OriginalFilename.Value.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
            {
                return 2;
            }
            if (document.OCRContent.Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
            {
                return 1;
            }
            return 0;
        }

        /// <inheritdoc />
        public GRYLibrary.Core.APIServer.CommonDBTypes.Role GetRoleByName(string roleName)
        {
            return this._TransientAuthenticationServicePersistence.GetRoleByName(roleName);
        }

        /// <inheritdoc />
        public bool DeleteIsAllowed(string documentId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SoftDelete(string documentId)
        {
            Document document = this.GetDocument(documentId);
            document.IsSoftDeleted = true;
        }

        /// <inheritdoc />
        public void SetAISummary(string documentId, string? shortSummary, string? longSummary)
        {
            Document document = this.GetDocument(documentId);
            document.AISummaryShort = shortSummary;
            document.AISummaryLong = longSummary;
        }

        /// <inheritdoc />
        public void AddDocumentVersion(DocumentVersionEntry versionEntry)
        {
            lock (_Lock)
            {
                this._DocumentVersions.Add(versionEntry);
                if (this._Documents.TryGetValue(versionEntry.ContentId, out Document? document))
                {
                    document.VersionNumber = versionEntry.Version;
                    document.VersionTimestamp = versionEntry.Timestamp;
                }
            }
        }

        /// <inheritdoc />
        public IReadOnlyList<DocumentVersionEntry> GetVersionsOfDocument(string documentId)
        {
            lock (_Lock)
            {
                return this._DocumentVersions
                    .Where(versionEntry => versionEntry.DocumentId == documentId)
                    .OrderBy(versionEntry => versionEntry.Version)
                    .ToList();
            }
        }

        /// <inheritdoc />
        public DocumentVersionEntry? GetVersionByContentId(string contentId)
        {
            lock (_Lock)
            {
                return this._DocumentVersions.FirstOrDefault(versionEntry => versionEntry.ContentId == contentId);
            }
        }

        /// <inheritdoc />
        public void SetIsLatestVersion(string contentId, bool isLatestVersion)
        {
            lock (_Lock)
            {
                if (this._Documents.TryGetValue(contentId, out Document? document))
                {
                    document.IsLatestVersion = isLatestVersion;
                }
            }
        }

        /// <inheritdoc />
        public string? GetSetting(string key)
        {
            lock (_Lock)
            {
                return this._Settings.TryGetValue(key, out string? value) ? value : null;
            }
        }

        /// <inheritdoc />
        public void SetSetting(string key, string value)
        {
            lock (_Lock)
            {
                this._Settings[key] = value;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetIdsOfDocumentsWhichMustBeHardDeletedNow()
        {
            DateTimeOffset now = this._TimeService.GetCurrentLocalTimeAsDateTimeOffset();
            //a document must be hard-deleted now exactly if it has a retention-deadline (MustBeHardDeletedAfter) which has been reached and it is not already hard-deleted; documents without a deadline are never deleted automatically.
            return this._Documents
                   .Where(doc => !doc.Value.IsHardDeleted && doc.Value.MustBeHardDeletedAfter != null && doc.Value.MustBeHardDeletedAfter.Value <= now)
                   .Select(doc => doc.Value.Id)
                   .ToList();
        }


        /// <inheritdoc />
        public void SetLogConnectionAttemptErrors(bool enabled)
        {
            lock (_Lock)
            {
                this._LogConnectionErrors = enabled;
            }
        }

        /// <inheritdoc />
        public GRYLibrary.Core.APIServer.CommonDBTypes.Role GetRoleById(string roleId)
        {
            return this._TransientAuthenticationServicePersistence.GetRoleById(roleId);
        }

        /// <inheritdoc />
        public void AddAccessToken(AccessToken newAccessToken)
        {
            this._TransientAuthenticationServicePersistence.AddAccessToken(newAccessToken);
        }

        /// <inheritdoc />
        public Model.BusinessTypes.User? GetUserByExternalLogin(string providerId, string subject)
        {
            lock (_Lock)
            {
                foreach (Model.BusinessTypes.User user in this._TransientAuthenticationServicePersistence.GetAllUsers().Values)
                {
                    if (user.ExternalLoginProvider == providerId && user.ExternalLoginSubject == subject)
                    {
                        return user;
                    }
                }
                return null;
            }
        }

        /// <inheritdoc />
        public bool UserWithExternalLoginExists(string providerId, string subject)
        {
            return this.GetUserByExternalLogin(providerId, subject) != null;
        }

        /// <inheritdoc />
        public void CreateMetadataFieldDefinition(MetadataFieldDefinition definition)
        {
            lock (_Lock)
            {
                this._MetadataFieldDefinitions[definition.Id] = definition;
            }
        }

        /// <inheritdoc />
        public void DeleteMetadataFieldDefinition(string fieldDefinitionId)
        {
            lock (_Lock)
            {
                this._MetadataFieldDefinitions.Remove(fieldDefinitionId);
                //remove the value every document holds for the deleted field.
                foreach (Document document in this._Documents.Values)
                {
                    document.MetadataValues.Remove(fieldDefinitionId);
                }
            }
        }

        /// <inheritdoc />
        public MetadataFieldDefinition GetMetadataFieldDefinition(string fieldDefinitionId)
        {
            lock (_Lock)
            {
                if (this._MetadataFieldDefinitions.TryGetValue(fieldDefinitionId, out MetadataFieldDefinition? definition))
                {
                    return definition;
                }
                throw new KeyNotFoundException($"No metadata-field-definition found with id '{fieldDefinitionId}'.");
            }
        }

        /// <inheritdoc />
        public IEnumerable<MetadataFieldDefinition> GetMetadataFieldDefinitionsOfStorageLocation(string storageLocationId)
        {
            lock (_Lock)
            {
                return this._MetadataFieldDefinitions.Values.Where(definition => definition.StorageLocationId == storageLocationId).ToList();
            }
        }

        /// <inheritdoc />
        public void SetDocumentMetadataValue(string documentId, string fieldDefinitionId, string value)
        {
            lock (_Lock)
            {
                this.GetDocument(documentId).MetadataValues[fieldDefinitionId] = value;
            }
        }

        /// <inheritdoc />
        public void RemoveDocumentMetadataValue(string documentId, string fieldDefinitionId)
        {
            lock (_Lock)
            {
                this.GetDocument(documentId).MetadataValues.Remove(fieldDefinitionId);
            }
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetMetadataValuesOfDocument(string documentId)
        {
            lock (_Lock)
            {
                return new Dictionary<string, string>(this.GetDocument(documentId).MetadataValues);
            }
        }
    }
}