using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.ConcreteEnvironments;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Miscellaneous;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OpenDMSBackend.Core.Services
{
    public class BusinessLogicService : IBusinessLogicService
    {
        private static readonly object _LockObject = new object();
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _Configuration;
        private readonly IPersistence _Persistence;
        private readonly IAuthenticationService<OpenDMSBackend.Core.Model.BusinessTypes.User> _AuthenticationService;
        private readonly ITimeService _TimeService;
        private readonly IApplicationConstants<CodeUnitSpecificConstants> _Constants;
        private readonly IGeneralLogger _Logger;
        private readonly IOCRService _OCRService;
        public BusinessLogicService(IPersistence persistence, IAuthenticationService<OpenDMSBackend.Core.Model.BusinessTypes.User> authenticationService, ITimeService timeService, IApplicationConstants<CodeUnitSpecificConstants> constants, IGeneralLogger logger, IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration, IOCRService oCRService)
        {
            this._Persistence = persistence;
            this._AuthenticationService = authenticationService;
            this._TimeService = timeService;
            this._Constants = constants;
            this._Logger = logger;
            this._Configuration = configuration;
            this._OCRService = oCRService;
        }

        public string AddDocument(string requesterUserId, string? title, string containerId, string originalFilename, byte[] content)
        {
            Document document = new Document(Guid.NewGuid().ToString(), title == null ? OneLineString.From(originalFilename) : OneLineString.From(title), OneLineString.From(originalFilename), OneLineString.From(originalFilename), this._TimeService.GetCurrentTimeAsGRYDateTime(), null, this._Persistence.GetNewReadableId(), OneLineString.From(OpenDMSBackend.Core.Miscellaneous.Utilities.GetMIMEType(originalFilename)), content, default, new HashSet<Tag>(), default);
            AnalyseDocument(document);
            this._Persistence.CreateDocument(document);
            this._Persistence.SetParentOfContainee(document.Id, containerId);
            this._Logger.Log($"Document {document.ReadableId} added.", Microsoft.Extensions.Logging.LogLevel.Information);
            return document.Id;
        }

        public string Register(string username, string password)
        {
            lock (_LockObject)
            {
                if (!this._Configuration.ApplicationSpecificConfiguration.RegistrationIsEnabled)
                {
                    throw new NotAuthorizedException();
                }
                OpenDMSBackend.Core.Model.BusinessTypes.User newUser = OpenDMSBackend.Core.Model.BusinessTypes.User.Create(username, password == null ? null : this._AuthenticationService.Hash(password), this._TimeService);
                this._AuthenticationService.AddUserTyped(newUser);

                Role userRole = this._AuthenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameUsers);
                this._AuthenticationService.EnsureUserHasRole(newUser.Id, userRole.Id);

                this._Logger.Log($"User '{username}' registered.", Microsoft.Extensions.Logging.LogLevel.Information);
                return newUser.Id;
            }
        }

        public Document GetDocument(string requesterUserId, string id)
        {
            //TODO do permission check
            return this._Persistence.GetDocument(id);
        }
        private DocumentPreview GetDocumentPreview(string requesterUserId, string id)
        {
            //TODO do permission check
            return this._Persistence.GetDocumentPreview(id);
        }


        public IEnumerable<DocumentPreview> Search(string requesterUserId, string searchTerm)
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
            IEnumerable<DocumentPreview> result = new List<DocumentPreview>();
            if (searchTerms.Length != 0)
            {
                throw new NotImplementedException();//TODO search and add to result
            }
            result = result.Where(document => this.UserIsAllowedToViewDocument(requesterUserId, document.Id));
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

        public bool UserIsAllowedToViewDocument(string userId, string documentId)
        {
            string storageLocationId = this._Persistence.GetStorageLocationId(documentId);
            if (this.UserIsAllowedToViewStorageLocation(userId, storageLocationId))
            {
                return true;
            }
            //add more possibilities if desired
            return false;
        }

        public bool UserIsAllowedToViewStorageLocation(string userId, string storageLocationId)
        {
            if (OpenDMSBackend.Core.Miscellaneous.Utilities.GetEnvironmentTargetType() is not Productive && this.UserIsAdministrator(userId))
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

        public TagDTO[] GetAllTags()
        {
            return this._Persistence.GetAllTags();
        }

        public IEnumerable<DocumentPreview> GetLatestDocuments(string requesterUserId)
        {
            return this._Persistence
                .GetAllDocumentIds()
                .Where(documentId => this.UserIsAllowedToViewDocument(requesterUserId, documentId))
                .Select(id => this.GetDocumentPreview(requesterUserId, id))
                .OrderByDescending(document => document.LastEditDate)
                .Take(10);
        }

        public void Update(string requesterUserId, Document updatedDocument)
        {
            //TODO check permission
            var existingDocument = this._Persistence.GetDocument(updatedDocument.Id);
            if ((existingDocument.MIMEType != updatedDocument.MIMEType) || (existingDocument.Content != updatedDocument.Content))
            {
                AnalyseDocument(updatedDocument);
            }
            this._Persistence.Update(requesterUserId, updatedDocument);
        }

        private void AnalyseDocument(Document document)
        {
            _Logger.Log($"Analyse document {document.ReadableId}", Microsoft.Extensions.Logging.LogLevel.Information);
            document.Preview = OpenDMSBackend.Core.Miscellaneous.Utilities.GeneratePreview(document.Content, document.MIMEType.Value);
            document.OCRContent = _OCRService.GetOCRContent(document.Content, document.MIMEType.Value);
        }

        public bool UserIsAllowedToEditDocument(string userId, string documentId)
        {
            throw new NotImplementedException();
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
            this._Persistence.SetParentOfContainee(id, parentContainerId);
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
            this._Persistence.Delete(containerOrContaineeId);
        }

        public void Move(string requesterUserId, string containeeIdToMove, string targetContainerId)
        {
            //TODO check permission
            this._Persistence.SetParentOfContainee(containeeIdToMove, targetContainerId);
        }

        public bool UserIsAdministrator(string userId)
        {
            return this._AuthenticationService.GetUser(userId).GetAllRoles().Where(role => role.Name == Constants.CodeUnitSpecificConstants.RolenameAdmins).Any();
        }
    }
}
