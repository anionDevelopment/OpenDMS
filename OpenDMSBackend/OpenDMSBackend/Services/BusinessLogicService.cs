using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
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

        public void AddDocument(string requesterUserId, string? title, string containerId, string originalFilename, byte[] content)
        {
            Document document = new Document(Guid.NewGuid().ToString(), title == null ? OneLineString.From(originalFilename) : OneLineString.From(title), OneLineString.From(originalFilename), OneLineString.From(originalFilename), this._TimeService.GetCurrentTimeAsGRYDateTime(), null, this._Persistence.GetNewReadableId(), content, Utilities.GeneratePreview(content), new HashSet<Tag>(), this._OCRService.GetOCRContent(content));
            this._Persistence.CreateDocument(document);
            this._Logger.Log($"Document {document.ReadableId} added.", Microsoft.Extensions.Logging.LogLevel.Information);
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

        public void CreateTag(string tagName, Color tagColor)
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
            throw new NotImplementedException();
        }

        public TagDTO[] GetAllTags()
        {
            return _Persistence.GetAllTags();
        }

        public IEnumerable<DocumentPreview> GetLatestDocuments(string requesterUserId)
        {
            return _Persistence
                .GetAllDocumentIds()
                .Where(documentId => this.UserIsAllowedToViewDocument(requesterUserId, documentId))
                .Select(d=>this.GetDocument(requesterUserId,d))
                .OrderByDescending(document => document.LastEditDate)
                .Take(10)
                .Select(document=>document.GetPreview());
        }
        public void Update(string requesterUserId, Document updatedDocument)
        {
            throw new NotImplementedException();
        }

        public bool UserIsAllowedToEditDocument(string userId, string documentId)
        {
            throw new NotImplementedException();
        }

        public void AddStorageLocation(string requesterUserId, string name)
        {
            throw new NotImplementedException();
        }

        public void AddFolder(string requesterUserId, string name, string parentContainerId)
        {
            throw new NotImplementedException();
        }

        public void Rename(string requesterUserId, string containerId, string newName)
        {
            //TODO check permission
            throw new NotImplementedException();
        }

        public void AuthorizeUserToViewStorageLocation(string requesterUserId, string storageLocationId, string sharedWithUserId)
        {
            throw new NotImplementedException();
        }

        public void UnauthorizeUserToViewStorageLocation(string requesterUserId, string storageLocationId, string sharedWithUserId)
        {
            throw new NotImplementedException();
        }

        public void Delete(string requesterUserId, string containerOrContaineeId)
        {
            throw new NotImplementedException();
        }

        public void Move(string requesterUserId, string containeeIdToMove, string targetContainerId)
        {
            throw new NotImplementedException();
        }
    }
}
