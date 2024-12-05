using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc.Strings;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Miscellaneous;
using OpenDMSBackend.Core.Model;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public class BusinessLogicService : IBusinessLogicService
    {
        private static readonly object _LockObject = new object();
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _Configuration;
        private readonly IPersistence _Persistence;
        private readonly IAuthenticationService<Model.User> _AuthenticationService;
        private readonly ITimeService _TimeService;
        private readonly IApplicationConstants<CodeUnitSpecificConstants> _Constants;
        private readonly IGeneralLogger _Logger;
        private readonly IOCRService _OCRService;
        public BusinessLogicService(IPersistence persistence, IAuthenticationService<Model.User> authenticationService, ITimeService timeService, IApplicationConstants<CodeUnitSpecificConstants> constants, IGeneralLogger logger, IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration,IOCRService oCRService)
        {
            this._Persistence = persistence;
            this._AuthenticationService = authenticationService;
            this._TimeService = timeService;
            this._Constants = constants;
            this._Logger = logger;
            this._Configuration = configuration;
            _OCRService= oCRService;
        }

        public void AddDocument(string? title, string originalFilename, byte[] content)
        {
            Document document = new Document(Guid.NewGuid().ToString(), title == null ? OneLineString.From(originalFilename) : OneLineString.From(title), OneLineString.From(originalFilename), OneLineString.From(originalFilename), _TimeService.GetCurrentTimeAsGRYDateTime(), null, _Persistence.GetNewReadableId(), content, Utilities.GeneratePreview(content), new HashSet<Tag>(), _OCRService.GetOCRContent(content));
            _Persistence.CreateDocument(document);
        }

        public string Register(string username, string password)
        {
            lock (_LockObject)
            {
                if (!this._Configuration.ApplicationSpecificConfiguration.RegistrationIsEnabled)
                {
                    throw new NotAuthorizedException();
                }
                Model.User newUser = Model.User.Create(username, password == null ? null : this._AuthenticationService.Hash(password), this._TimeService);
                this._AuthenticationService.AddUserTyped(newUser);
                return newUser.Id;
            }
        }

        public Document GetDocument(string id)
        {
            //TODO do permission check
            return _Persistence.GetDocument(id);
        }

        public IEnumerable<DocumentPreview> Search(string searchTerm)
        {
            throw new NotImplementedException();
        }

        public bool UserWithNameExists(string username)
        {
            return _Persistence.UserWithNameExists(username);
        }

        public void CreateTag(string tagName, Color tagColor)
        {
            //TODO do permission check
            _Persistence.CreateTag(new Tag(Guid.NewGuid().ToString(),tagName, tagColor));
        }

        public void AssignTag(string documentId, string tagId)
        {
            //TODO do permission check
            _Persistence.AssignTag( documentId,  tagId);
        }

        public void UnassignTag(string documentId, string tagId)
        {
            //TODO do permission check
            _Persistence.UnassignTag(documentId, tagId);
        }
    }
}
