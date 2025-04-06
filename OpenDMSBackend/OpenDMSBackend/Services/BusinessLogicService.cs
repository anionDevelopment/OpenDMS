using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Res;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.BusinessTypes.DocumentTypes;
using OpenDMSBackend.Core.Model.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDMSBackend.Core.Services
{
    public class BusinessLogicService : IBusinessLogicService
    {
        private static readonly object _LockObject = new object();
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _Configuration;
        private readonly IPersistence _Persistence;
        private readonly IAuthenticationService<Model.BusinessTypes.User> _AuthenticationService;
        private readonly ITimeService _TimeService;
        private readonly IApplicationConstants<CodeUnitSpecificConstants> _Constants;
        private readonly IGeneralLogger _Logger;
        private readonly IOCRService _OCRService;
        private readonly IIdGenerator<ulong> _IdGenerator;
        private readonly IGeneralResourceLoader _GeneralResourceLoader;
        public BusinessLogicService(IPersistence persistence, IAuthenticationService<Model.BusinessTypes.User> authenticationService, ITimeService timeService, IApplicationConstants<CodeUnitSpecificConstants> constants, IGeneralLogger logger, IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration, IOCRService oCRService, IIdGenerator<ulong> idGenerator, IGeneralResourceLoader generalResourceLoader)
        {
            this._Persistence = persistence;
            this._AuthenticationService = authenticationService;
            this._TimeService = timeService;
            this._Constants = constants;
            this._Logger = logger;
            this._Configuration = configuration;
            this._OCRService = oCRService;
            this._IdGenerator = idGenerator;
            this._GeneralResourceLoader = generalResourceLoader;
        }

        public string AddDocument(string requesterUserId, string? title, string containerId, string originalFilename, byte[] content, GRYDateTime creationDate)
        {
            lock (_LockObject)
            {
                Document document = new Document(Guid.NewGuid().ToString(), title == null ? OneLineString.From(originalFilename) : OneLineString.From(title), OneLineString.From(originalFilename), OneLineString.From(originalFilename), creationDate, null, this._IdGenerator.GenerateNewId(), new HashSet<Tag>(), OneLineString.From(Miscellaneous.Utilities.GetMIMEType(originalFilename)), default, default, content);
                this.AnalyseDocument(document);
                this._Persistence.CreateDocument(document);
                this._Persistence.SetParentOfContainee(document, containerId);
                this._Logger.Log($"Document '{document.ReadableId}' added. (Technical-id: {document.Id})", Microsoft.Extensions.Logging.LogLevel.Information);
                return document.Id;
            }
        }

        public string Register(string username, string password)
        {
            lock (_LockObject)
            {
                if (!this._Configuration.ApplicationSpecificConfiguration.RegistrationIsEnabled)
                {
                    throw new NotAuthorizedException();
                }
                Model.BusinessTypes.User newUser = Model.BusinessTypes.User.Create(username, password == null ? null : this._AuthenticationService.Hash(password), this._TimeService);
                this._AuthenticationService.AddUserTyped(newUser);

                Role userRole = this._AuthenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameUsers);
                this._AuthenticationService.EnsureUserHasRole(newUser.Id, userRole.Id);

                this._Logger.Log($"User '{newUser.Name}' registered. (Technical-id: {newUser.Id})", Microsoft.Extensions.Logging.LogLevel.Information);
                return newUser.Id;
            }
        }

        public Document GetDocument(string requesterUserId, string id)
        {
            this.EnsureUserIsAllowedToViewContent(requesterUserId, id);
            return this._Persistence.GetDocument(id);
        }

        public DocumentPreview GetDocumentPreview(string requesterUserId, string documentId)
        {
            this.EnsureUserIsAllowedToViewContent(requesterUserId, documentId);
            return this._Persistence.GetDocumentPreview(documentId);
        }

        private void EnsureUserIsAllowedToViewContent(string requesterUserId, string contentId)
        {
            if (!this.UserIsAllowedToViewContent(requesterUserId, contentId))
            {
                throw new NotAuthorizedException($"No permission to view document '{contentId}'.");
            }
        }

        public bool UserIsAllowedToEditContent(string userId, string documentId)
        {
            throw new NotImplementedException();
        }

        public IList<DocumentPreview> Search(string requesterUserId, string searchTerm)
        {
            string[] searchTerms;
            if (searchTerm.Contains(' '))
            {
                searchTerms = searchTerm.Split(' ');
            }
            else
            {
                searchTerms = new string[] { searchTerm };
            }
            searchTerms = searchTerms.Select(searchTerm => searchTerm.Trim()).Where(searchTerm => !string.IsNullOrEmpty(searchTerm)).ToArray();
            IList<DocumentPreview> result = new List<DocumentPreview>();
            if (searchTerms.Length != 0)
            {
                result = this._Persistence.Search(requesterUserId, searchTerms);
            }
            result = result.Where(document => this.UserIsAllowedToViewContent(requesterUserId, document.Id)).ToList();
            return result;
        }

        public bool UserWithNameExists(string username)
        {
            return this._Persistence.UserWithNameExists(username);
        }

        public void CreateTag(string tagName, ExtendedColor tagColor)
        {
            //TODO do permission check
            this._Persistence.CreateTag(new Tag(Guid.NewGuid().ToString(), tagName, tagColor));
        }

        public void AssignTag(string documentId, string tagId)
        {
            //TODO do permission check
            this._Persistence.AssignTag(documentId, tagId);
        }

        public void UnassignTag(string documentId, string tagId)
        {
            //TODO do permission check
            this._Persistence.UnassignTag(documentId, tagId);
        }

        public bool UserIsAllowedToViewContent(string userId, string contentId)
        {
            return Miscellaneous.Utilities.DoForContentObject(this._Persistence, contentId,
                (storageLocationId) => this.UserIsAllowedToViewStorageLocation(userId, storageLocationId),
                (folderId) => this.UserIsAllowedToViewFolder(userId, folderId),
                (documentId) => this.UserIsAllowedToViewDocument(userId, documentId)
            );
        }

        public bool UserIsAllowedToViewStorageLocation(string userId, string storageLocationId)
        {
            if (Miscellaneous.Utilities.GetEnvironmentTargetType() is not Productive && this.UserIsAdministrator(userId))
            {
                return true;
            }
            if (this._Persistence.UserIsOwnerOfStorageLocation(userId, storageLocationId))
            {
                return true;
            }
            if (this._Persistence.StorageLocationIsSharedWithUser(storageLocationId, userId))
            {
                return true;
            }
            //add more possibilities if desired
            return false;
        }

        public bool UserIsAllowedToViewFolder(string userId, string contentId)
        {
            string storageLocationId = this._Persistence.GetIdOfStorageLocationContainedIn(contentId);
            if (this.UserIsAllowedToViewStorageLocation(userId, storageLocationId))
            {
                return true;
            }
            //add more possibilities if desired
            return false;
        }

        public bool UserIsAllowedToViewDocument(string userId, string contentId)
        {
            string storageLocationId = this._Persistence.GetIdOfStorageLocationContainedIn(contentId);
            if (this.UserIsAllowedToViewStorageLocation(userId, storageLocationId))
            {
                return true;
            }
            //add more possibilities if desired
            return false;
        }

        public TagDTO[] GetAllTags()
        {
            return this._Persistence.GetAllTags();
        }

        public IEnumerable<DocumentPreview> GetLatestDocuments(string requesterUserId)
        {
            List<DocumentPreview> result = this._Persistence
                .GetAllDocumentIds()
                .Where(documentId => this.UserIsAllowedToViewContent(requesterUserId, documentId))
                .Select(id => this.GetDocumentPreview(requesterUserId, id))
                .OrderByDescending(document => document.GetNewestDate(document))
                .Take(5)
                .ToList();
            return result;
        }

        public void Update(string requesterUserId, Document updatedDocument)
        {
            //TODO check permission
            Document existingDocument = this._Persistence.GetDocument(updatedDocument.Id);
            if ((existingDocument.MIMEType != updatedDocument.MIMEType) || (existingDocument.Content != updatedDocument.Content))
            {
                this.AnalyseDocument(updatedDocument);
            }
            this._Persistence.Update(requesterUserId, updatedDocument);
        }

        private void AnalyseDocument(Document document)
        {
            this._Logger.Log($"Analyse document {document.ReadableId}", Microsoft.Extensions.Logging.LogLevel.Information);
            DocumentType docType = Miscellaneous.Utilities.GetDocumentType(document.MIMEType.Value);

            byte[] noPreviewAvailablePicture = this._GeneralResourceLoader.GetResource("NoPreviewAvailablePicture.jpg");
            try
            {
                if (docType is Unknown)
                {
                    document.Preview = noPreviewAvailablePicture;
                }
                else
                {
                    document.Preview = docType.GetPreview(document.Content);
                }
            }
            catch
            {
                document.Preview = noPreviewAvailablePicture;
            }

            string noOCRContentAvailableResult = string.Empty;
            try
            {
                if (docType is Unknown)
                {
                    document.OCRContent = noOCRContentAvailableResult;
                }
                else
                {
                    document.OCRContent = docType.GetOCRContent(document.Content, this._OCRService);
                }
            }
            catch
            {
                document.OCRContent = noOCRContentAvailableResult;
            }
        }

        public string AddStorageLocation(string requesterUserId, string name)
        {
            //TODO check permission
            string id = this._Persistence.AddStoragLocation(name);
            this._Persistence.SetOwnerOfStorageLocation(id, requesterUserId);
            return id;
        }

        public string AddFolder(string requesterUserId, string name, string parentContainerId)
        {
            //TODO check permission
            string id = this._Persistence.AddFolder(name);
            this._Persistence.SetParentOfContainee(this.GetContainee(id), parentContainerId);
            this._Logger.Log($"Folder '{name}' added. (Technical-id: {id})", Microsoft.Extensions.Logging.LogLevel.Information);
            return id;
        }

        public void Rename(string requesterUserId, string containerId, string newName)
        {
            //TODO check permission
            this._Persistence.Rename(containerId, newName);
        }

        public void AuthorizeUserToViewStorageLocation(string requesterUserId, string storageLocationId, string sharedWithUserId)
        {
            //TODO check permission
            this._Persistence.AuthorizeUserToViewStorageLocation(storageLocationId, sharedWithUserId);
        }

        public void UnauthorizeUserToViewStorageLocation(string requesterUserId, string storageLocationId, string sharedWithUserId)
        {
            //TODO check permission
            this._Persistence.UnauthorizeUserToViewStorageLocation(storageLocationId, sharedWithUserId);
        }

        public void Delete(string requesterUserId, string containerOrContaineeId)
        {
            //TODO check permission

            //remove from parent container
            if (this._Persistence.IsContaineeId(containerOrContaineeId))
            {
                string parentId = this._Persistence.GetParentIdOfContainee(containerOrContaineeId);
                this._Persistence.RemoveChild(parentId, containerOrContaineeId);
            }

            //remove content
            Miscellaneous.Utilities.DoForContentObject(this._Persistence, containerOrContaineeId, (storageLocationId) => this.RemoveEntireContent(requesterUserId, storageLocationId), (folderId) => this.RemoveEntireContent(requesterUserId, folderId), null);

            this._Persistence.Delete(containerOrContaineeId);
        }

        public void Move(string requesterUserId, string containeeIdToMove, string targetContainerId)
        {
            //TODO check permission
            //TODO remove containeeToMove from previous parent
            this._Persistence.SetParentOfContainee(this.GetContainee(containeeIdToMove), targetContainerId);
        }

        private IContainee GetContainee(string containeeId)
        {
            return Miscellaneous.Utilities.DoForContentObject<IContainee>(this._Persistence, containeeId,
                (storageLocationId) => { throw new NotSupportedException(); },
                (folderId) => { return this._Persistence.GetFolder(containeeId); },
                (documentId) => { return this._Persistence.GetDocument(containeeId); }
            );
        }

        public bool UserIsAdministrator(string userId)
        {
            return this._AuthenticationService.GetUser(userId).GetAllRoles().Where(role => role.Name == CodeUnitSpecificConstants.RolenameAdmins).Any();
        }

        public IEnumerable<StorageLocation> GetAllViewableStorageLocations(string requesterUserId)
        {
            List<StorageLocation> result = this._Persistence.GetAllStorageLocationIds().Where(storageLocationId => this.UserIsAllowedToViewStorageLocation(requesterUserId, storageLocationId)).Select(storageLocationId => this._Persistence.GetStorageLocation(storageLocationId)).ToList();
            return result;
        }

        public Folder GetFolder(string requesterUserId, string folderId)
        {
            this.EnsureUserIsAllowedToViewContent(requesterUserId, folderId);
            return this._Persistence.GetFolder(folderId);
        }

        public void Housekeeping()
        {
            throw new NotImplementedException();//TODO remove expired accesstoken
        }

        public void RemoveEntireContent(string requesterUserId, string containerId)
        {
            IContainer container = this._Persistence.GetContainerById(containerId);
            foreach (IContainee child in container.Content)
            {
                this.Delete(requesterUserId, child.Id);
            }
        }

        public Document GetDocumentFromReadableId(string requesterUserId, uint readableId)
        {
            return this.GetDocument(requesterUserId, this._Persistence.GetIdFromReadableId(readableId));
        }
    }
}
