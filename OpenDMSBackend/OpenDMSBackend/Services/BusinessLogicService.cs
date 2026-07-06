using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
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
using OpenDMSBackend.Core.Model.DTOs;
using SimpleOCR.Library.Core.FileTypes;
using SimpleOCR.Library.Core.Visitors;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private readonly IOCRServiceClient _OCRService;
        private readonly IAISummaryServiceClient _AISummaryService;
        private readonly IIdGenerator<ulong> _IdGenerator;
        private readonly IGeneralResourceLoader _GeneralResourceLoader;
        private readonly IAuditLog _AuditLog;
        /// <summary>Initializes a new instance of <see cref="BusinessLogicService"/>.</summary>
        /// <param name="persistence">The persistence service used to store and retrieve data.</param>
        /// <param name="authenticationService">The authentication service for user management.</param>
        /// <param name="timeService">The service used to obtain the current time.</param>
        /// <param name="constants">Application-wide constants.</param>
        /// <param name="logger">The general-purpose logger.</param>
        /// <param name="configuration">The persisted server configuration.</param>
        /// <param name="oCRService">The OCR service client.</param>
        /// <param name="aISummaryService">The AI-summary service client.</param>
        /// <param name="idGenerator">Generator for unique numeric identifiers.</param>
        /// <param name="generalResourceLoader">Loader for embedded general resources.</param>
        /// <param name="auditLog">The audit log service.</param>
        public BusinessLogicService(IPersistence persistence, IAuthenticationService<Model.BusinessTypes.User> authenticationService, ITimeService timeService, IApplicationConstants<CodeUnitSpecificConstants> constants, IServerLog logger, IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration, IOCRServiceClient oCRService, IAISummaryServiceClient aISummaryService, IIdGenerator<ulong> idGenerator, IGeneralResourceLoader generalResourceLoader, IAuditLog auditLog)
        {
            this._Persistence = persistence;
            this._AuthenticationService = authenticationService;
            this._TimeService = timeService;
            this._Constants = constants;
            this._Logger = logger.Logger;
            this._Configuration = configuration;
            this._OCRService = oCRService;
            this._AISummaryService = aISummaryService;
            this._IdGenerator = idGenerator;
            this._GeneralResourceLoader = generalResourceLoader;
            this._AuditLog = auditLog;
        }

        /// <summary>Creates and persists a new document in the specified container.</summary>
        /// <param name="requesterUserId">The id of the user requesting the operation.</param>
        /// <param name="title">The display title of the document, or <see langword="null"/> to use the original filename.</param>
        /// <param name="containerId">The id of the container (folder or storage location) to place the document in.</param>
        /// <param name="originalFilename">The original filename including extension.</param>
        /// <param name="content">The raw byte content of the document.</param>
        /// <param name="groupOfBusinessOwner">The business-owner group associated with the document.</param>
        /// <param name="additionalOCRLanguages">Additional ISO-639-1 language codes to use during OCR analysis.</param>
        /// <returns>The id of the newly created document.</returns>
        public string AddDocument(string? requesterUserId, string? title, string containerId, string originalFilename, byte[] content, string groupOfBusinessOwner, ISet<string> additionalOCRLanguages)
        {
            //adding a document changes the target-container, so the requesting user must be allowed to change it. Automatic imports (see issue #11 / ManagementService) pass no requesting user and are always allowed.
            if (requesterUserId != null)
            {
                this.EnsureUserIsAllowedToEditContent(requesterUserId, containerId);
            }
            lock (_LockObject)
            {
                Document document = this.CreateAndPersistAnalysedDocument(requesterUserId, title, containerId, originalFilename, content, groupOfBusinessOwner, additionalOCRLanguages);
                this.RegisterAsNewVersion(document, null);
                this.GenerateAISummaryIfAutoGenerationIsEnabled(document);
                this._AuditLog.Logger.Log($"Document '{document.Id}' (readable-id {document.ReadableId}) added to container '{containerId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
                return document.Id;
            }
        }

        /// <summary>Creates, analyses and persists a single document-row (one version) in the given container, without registering it in the version-table.</summary>
        private Document CreateAndPersistAnalysedDocument(string? requesterUserId, string? title, string containerId, string originalFilename, byte[] content, string groupOfBusinessOwner, ISet<string> additionalOCRLanguages)
        {
            Document document = new Document(Guid.NewGuid().ToString(), title == null ? OneLineString.From(originalFilename) : OneLineString.From(title), OneLineString.From(originalFilename), OneLineString.From(originalFilename), this._TimeService.GetCurrentLocalTimeAsDateTimeOffset(), this._IdGenerator.GenerateNewId(), new HashSet<Tag>(), OneLineString.From(SimpleOCR.Library.Core.Misc.Utilities.GetMIMEType(originalFilename)), content, default!/*property will be set by AnalyseDocument(...)*/, default!/*property will be set by AnalyseDocument(...)*/, false, default, default, groupOfBusinessOwner, additionalOCRLanguages, requesterUserId);
            this.AnalyseDocument(document);
            this.Validate(document);
            this._Persistence.CreateDocument(document);
            this._Persistence.SetParentOfContainee(document, containerId);
            this._Logger.Log($"Document '{document.ReadableId}' added. (Technical-id: {document.Id})", Microsoft.Extensions.Logging.LogLevel.Information);
            return document;
        }

        /// <summary>Registers the given document-row as a version. If <paramref name="currentLatestId"/> is given, the row becomes the next version of that version-chain and the previous latest-version is unmarked; otherwise a new version-chain (version 1) is started.</summary>
        private void RegisterAsNewVersion(Document newVersionDocument, string? currentLatestId)
        {
            DateTimeOffset timestamp = this._TimeService.GetCurrentLocalTimeAsDateTimeOffset();
            string logicalDocumentId;
            int versionNumber;
            if (currentLatestId == null)
            {
                logicalDocumentId = Guid.NewGuid().ToString();
                versionNumber = 1;
            }
            else
            {
                DocumentVersionEntry? currentEntry = this._Persistence.GetVersionByContentId(currentLatestId);
                logicalDocumentId = currentEntry == null ? Guid.NewGuid().ToString() : currentEntry.DocumentId;
                versionNumber = (currentEntry == null ? 1 : currentEntry.Version) + 1;
                this._Persistence.SetIsLatestVersion(currentLatestId, false);
            }
            newVersionDocument.IsLatestVersion = true;
            newVersionDocument.VersionNumber = versionNumber;
            newVersionDocument.VersionTimestamp = timestamp;
            this._Persistence.AddDocumentVersion(new DocumentVersionEntry(logicalDocumentId, newVersionDocument.Id, versionNumber, timestamp));
        }

        /// <inheritdoc />
        public string UploadNewVersion(string? requesterUserId, string oldDocumentId, string? title, string originalFilename, byte[] content, string groupOfBusinessOwner, ISet<string> additionalOCRLanguages)
        {
            //uploading a new version changes the existing document, so the requesting user must be allowed to change it. Automatic imports pass no requesting user and are always allowed.
            if (requesterUserId != null)
            {
                this.EnsureUserIsAllowedToEditContent(requesterUserId, oldDocumentId);
            }
            lock (_LockObject)
            {
                string parentContainerId = this._Persistence.GetParentIdOfContainee(oldDocumentId);
                Document newVersion = this.CreateAndPersistAnalysedDocument(requesterUserId, title, parentContainerId, originalFilename, content, groupOfBusinessOwner, additionalOCRLanguages);
                this.RegisterAsNewVersion(newVersion, oldDocumentId);
                this.GenerateAISummaryIfAutoGenerationIsEnabled(newVersion);
                this._AuditLog.Logger.Log($"New version '{newVersion.Id}' (version {newVersion.VersionNumber}) of document '{oldDocumentId}' uploaded by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
                return newVersion.Id;
            }
        }

        /// <inheritdoc />
        public IEnumerable<DocumentPreview> GetVersionHistory(string requesterUserId, string documentId)
        {
            DocumentVersionEntry? entry = this._Persistence.GetVersionByContentId(documentId);
            if (entry == null)
            {
                //the document is not versioned: return only the document itself (if viewable).
                List<DocumentPreview> single = new List<DocumentPreview>();
                if (this.UserIsAllowedToViewContent(requesterUserId, documentId))
                {
                    single.Add(this._Persistence.GetDocumentPreview(documentId));
                }
                return single;
            }
            List<DocumentPreview> result = new List<DocumentPreview>();
            foreach (DocumentVersionEntry versionEntry in this._Persistence.GetVersionsOfDocument(entry.DocumentId))
            {
                if (this.UserIsAllowedToViewContent(requesterUserId, versionEntry.ContentId))
                {
                    result.Add(this._Persistence.GetDocumentPreview(versionEntry.ContentId));
                }
            }
            return result;
        }

        private void Validate(Document document)
        {
            if (!this.IsValid(document, out IList<string> errorMessages))
            {
                string messagesAsString = string.Join(", ", errorMessages.Select(message => "\"" + message + "\""));
                throw new BadRequestException($"Document is not valid due to the following reason(s): {messagesAsString}");
            }
        }

        private bool IsValid(Document document, out IList<string> errorMessages)
        {
            errorMessages = new List<string>();
            //TODO check if all assigned languages (if there are some) are valid iso-639-1-identifier
            return errorMessages.Count == 0;
        }

        /// <summary>Registers a new user account if registration is currently enabled.</summary>
        /// <param name="username">The desired username.</param>
        /// <param name="password">The plain-text password to hash and store.</param>
        /// <returns>The id of the newly created user.</returns>
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

                this._AuditLog.Logger.Log($"User with id {newUser.Id} registered.", Microsoft.Extensions.Logging.LogLevel.Information);
                return newUser.Id;
            }
        }

        /// <summary>Retrieves a document by its id, enforcing view-permission checks.</summary>
        /// <param name="requesterUserId">The id of the user requesting the document.</param>
        /// <param name="id">The id of the document to retrieve.</param>
        /// <returns>The requested <see cref="Document"/>.</returns>
        public Document GetDocument(string requesterUserId, string id)
        {
            this.EnsureUserIsAllowedToViewContent(requesterUserId, id);
            return this._Persistence.GetDocument(id);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool UserIsAllowedToEditContent(string userId, string contentId)
        {
            return Core.Misc.Utilities.DoForContentObject(this._Persistence, contentId,
                (storageLocationId) => this.UserIsAllowedToEditStorageLocation(userId, storageLocationId),
                (folderId) => this.UserIsAllowedToEditFolder(userId, folderId),
                (documentId) => this.UserIsAllowedToEditDocument(userId, documentId)
            );
        }

        /// <summary>Determines whether the given user may change the given storage-location and its contents. In contrast to merely viewing it, a storage-location may only be changed by an administrator or its owner; users it was only shared with (view-permission) may not change it.</summary>
        private bool UserIsAllowedToEditStorageLocation(string userId, string storageLocationId)
        {
            //default-deny (see issue #13): being an administrator does NOT grant the permission to change content. Only the moderator (owner) and users a moderator granted edit-permission may change the contents.
            if (this._Persistence.UserIsOwnerOfStorageLocation(userId, storageLocationId))
            {
                return true;
            }
            if (this._Persistence.StorageLocationIsEditableByUser(storageLocationId, userId))
            {
                return true;
            }
            return false;
        }

        private bool UserIsAllowedToEditFolder(string userId, string folderId)
        {
            return this.UserIsAllowedToEditStorageLocation(userId, this._Persistence.GetIdOfStorageLocationContainedIn(folderId));
        }

        private bool UserIsAllowedToEditDocument(string userId, string documentId)
        {
            return this.UserIsAllowedToEditStorageLocation(userId, this._Persistence.GetIdOfStorageLocationContainedIn(documentId));
        }

        /// <summary>Ensures the given user is allowed to change the given content and throws a <see cref="NotAuthorizedException"/> otherwise.</summary>
        private void EnsureUserIsAllowedToEditContent(string requesterUserId, string contentId)
        {
            if (!this.UserIsAllowedToEditContent(requesterUserId, contentId))
            {
                throw new NotAuthorizedException($"No permission to change '{contentId}'.");
            }
        }

        /// <summary>Ensures the operation is performed by an authenticated user and throws a <see cref="NotAuthorizedException"/> otherwise.</summary>
        private void EnsureAuthenticated(string? requesterUserId)
        {
            if (string.IsNullOrEmpty(requesterUserId))
            {
                throw new NotAuthorizedException("This operation requires an authenticated user.");
            }
        }

        /// <summary>Describes the initiator of an operation for audit-log-entries. Operations without a requesting user are automatic system-operations (for example imports or the scheduled hard-deletion).</summary>
        private static string DescribeRequester(string? requesterUserId)
        {
            return string.IsNullOrEmpty(requesterUserId) ? "an automatic system-operation" : $"user '{requesterUserId}'";
        }

        /// <summary>Ensures the given user has administrator-privileges and throws a <see cref="NotAuthorizedException"/> otherwise.</summary>
        private void EnsureAdministrator(string requesterUserId)
        {
            if (!this.UserIsAdministrator(requesterUserId))
            {
                throw new NotAuthorizedException("This operation requires administrator-privileges.");
            }
        }

        /// <summary>Ensures the given user is a moderator of the storage-location and throws a <see cref="NotAuthorizedException"/> otherwise. A moderator (currently the owner) is the only one who may manage a storage-location's permissions (see issue #13).</summary>
        private void EnsureUserIsModeratorOfStorageLocation(string requesterUserId, string storageLocationId)
        {
            if (!this._Persistence.UserIsOwnerOfStorageLocation(requesterUserId, storageLocationId))
            {
                throw new NotAuthorizedException($"Only a moderator (owner) of storage-location '{storageLocationId}' may manage its permissions.");
            }
        }

        /// <inheritdoc />
        public IList<DocumentPreview> Search(string requesterUserId, string searchTerm)
        {
            IList<string> searchResults = new List<string>();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchResults = this._Persistence.Search(searchTerm.ToLower());
            }
            return searchResults
                .Where(documentId => this.UserIsAllowedToViewContent(requesterUserId, documentId))
                .Select(this._Persistence.GetDocumentPreview)
                .Where(preview => preview.IsLatestVersion && !preview.IsHardDeleted)
                .ToList();
        }

        /// <inheritdoc />
        public bool UserWithNameExists(string username)
        {
            return this._Persistence.UserWithNameExists(username);
        }

        /// <inheritdoc />
        public void CreateTag(string requesterUserId, string tagName, ExtendedColor tagColor)
        {
            //creating a (globally usable) tag is allowed for any authenticated user.
            this.EnsureAuthenticated(requesterUserId);
            Tag tag = new Tag(Guid.NewGuid().ToString(), tagName, tagColor);
            this._Persistence.CreateTag(tag);
            this._AuditLog.Logger.Log($"Tag '{tagName}' (id '{tag.Id}') created by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <summary>Assigns an existing tag to an existing document. The requesting user must be allowed to change the document.</summary>
        /// <param name="requesterUserId">The id of the user performing the operation.</param>
        /// <param name="documentId">The id of the document.</param>
        /// <param name="tagId">The id of the tag to assign.</param>
        public void AssignTag(string requesterUserId, string documentId, string tagId)
        {
            this.EnsureUserIsAllowedToEditContent(requesterUserId, documentId);
            this._Persistence.AssignTag(documentId, tagId);
            this._AuditLog.Logger.Log($"Tag '{tagId}' assigned to document '{documentId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <summary>Removes a tag assignment from an existing document. The requesting user must be allowed to change the document.</summary>
        /// <param name="requesterUserId">The id of the user performing the operation.</param>
        /// <param name="documentId">The id of the document.</param>
        /// <param name="tagId">The id of the tag to unassign.</param>
        public void UnassignTag(string requesterUserId, string documentId, string tagId)
        {
            this.EnsureUserIsAllowedToEditContent(requesterUserId, documentId);
            this._Persistence.UnassignTag(documentId, tagId);
            this._AuditLog.Logger.Log($"Tag '{tagId}' unassigned from document '{documentId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public bool UserIsAllowedToViewContent(string userId, string contentId)
        {
            return Core.Misc.Utilities.DoForContentObject(this._Persistence, contentId,
                (storageLocationId) => this.UserIsAllowedToViewStorageLocation(userId, storageLocationId),
                (folderId) => this.UserIsAllowedToViewFolder(userId, folderId),
                (documentId) => this.UserIsAllowedToViewDocument(userId, documentId)
            );
        }

        /// <inheritdoc />
        public bool UserIsAllowedToViewStorageLocation(string userId, string storageLocationId)
        {
            //access-protection follows a default-deny concept (see issue #13): being an administrator does NOT grant access to content. Only the moderator (owner) of the storage-location and the users a moderator has explicitly granted view- or edit-permission may retrieve its contents.
            if (this._Persistence.UserIsOwnerOfStorageLocation(userId, storageLocationId))
            {
                return true;
            }
            if (this._Persistence.StorageLocationIsSharedWithUser(storageLocationId, userId))
            {
                return true;
            }
            return false;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public TagDTO[] GetAllTags()
        {
            return this._Persistence.GetAllTags();
        }

        /// <inheritdoc />
        public IEnumerable<DocumentPreview> GetLatestDocuments(string requesterUserId)
        {
            List<DocumentPreview> result = this._Persistence
                .GetAllDocumentIds()
                .Where(documentId => this.UserIsAllowedToViewContent(requesterUserId, documentId))
                .Select(id => this.GetDocumentPreview(requesterUserId, id))
                .Where(document => document.IsLatestVersion && !document.IsHardDeleted)
                .OrderByDescending(document => document.GetNewestDate(document))
                .Take(5)
                .ToList();
            return result;
        }

        /// <inheritdoc />
        public void UpdateDocumentTitle(string requesterUserId, string documentId, string newTitle)
        {
            lock (_LockObject)
            {
                //a metadata-change creates a new version whose content, OCR-content and AI-summary are copied from the current version (they are not recomputed).
                this.CreateMetadataVersion(requesterUserId, documentId, newVersion => newVersion.Title = OneLineString.From(newTitle));
                this._AuditLog.Logger.Log($"Title of document '{documentId}' changed to '{newTitle}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
            }
        }

        /// <summary>Creates a new version of the given document in which only metadata is changed (via <paramref name="mutate"/>). Content, preview, OCR-content and AI-summary are copied unchanged from the current version.</summary>
        private void CreateMetadataVersion(string requesterUserId, string currentDocumentId, Action<Document> mutate)
        {
            this.EnsureUserIsAllowedToEditContent(requesterUserId, currentDocumentId);
            Document current = this._Persistence.GetDocument(currentDocumentId);
            Document newVersion = new Document(Guid.NewGuid().ToString(), current.Title, current.Filename, current.OriginalFilename, this._TimeService.GetCurrentLocalTimeAsDateTimeOffset(), this._IdGenerator.GenerateNewId(), new HashSet<Tag>(current.Tags), current.MIMEType, current.Content, current.OCRContent, current.Preview, current.IsSoftDeleted, current.DeleteIsNotAllowedBefore, current.MustBeHardDeletedAfter, current.GroupOfBusinessOwner, new HashSet<string>(current.AssignedLanguages), current.AddedByUserId)
            {
                AISummaryShort = current.AISummaryShort,
                AISummaryLong = current.AISummaryLong,
            };
            mutate(newVersion);
            this.Validate(newVersion);
            this._Persistence.CreateDocument(newVersion);
            this._Persistence.SetParentOfContainee(newVersion, this._Persistence.GetParentIdOfContainee(currentDocumentId));
            this.RegisterAsNewVersion(newVersion, currentDocumentId);
        }

        /// <inheritdoc />
        public void Update(string requesterUserId, Document updatedDocument)
        {
            //the requesting user must be allowed to change the document. (Future refinement: only members of the GroupOfBusinessOwner may change the retention-dates DeleteIsNotAllowedBefore/MustBeHardDeletedAfter or hard-delete; see issue #13.)
            this.EnsureUserIsAllowedToEditContent(requesterUserId, updatedDocument.Id);
            //TODO check validity, for example: content must not be null, DeleteIsNotAllowedBefore must be lower or equal to MustBeHardDeletedAfter, etc.
            lock (_LockObject)
            {
                Document current = this._Persistence.GetDocument(updatedDocument.Id);
                bool contentChanged = !current.Content.SequenceEqual(updatedDocument.Content) || (current.MIMEType != updatedDocument.MIMEType) || (!current.AssignedLanguages.SetEquals(updatedDocument.AssignedLanguages));
                Document newVersion = new Document(Guid.NewGuid().ToString(), updatedDocument.Title, updatedDocument.Filename, updatedDocument.OriginalFilename, this._TimeService.GetCurrentLocalTimeAsDateTimeOffset(), this._IdGenerator.GenerateNewId(), new HashSet<Tag>(updatedDocument.Tags), updatedDocument.MIMEType, updatedDocument.Content, default!/*set below*/, default!/*set below*/, updatedDocument.IsSoftDeleted, updatedDocument.DeleteIsNotAllowedBefore, updatedDocument.MustBeHardDeletedAfter, updatedDocument.GroupOfBusinessOwner, new HashSet<string>(updatedDocument.AssignedLanguages), updatedDocument.AddedByUserId);
                if (contentChanged)
                {
                    //the content changed: OCR-content and preview are recomputed and the (now stale) AI-summary is dropped.
                    this.AnalyseDocument(newVersion);
                }
                else
                {
                    newVersion.OCRContent = current.OCRContent;
                    newVersion.Preview = current.Preview;
                    newVersion.AISummaryShort = current.AISummaryShort;
                    newVersion.AISummaryLong = current.AISummaryLong;
                }
                this.Validate(newVersion);
                this._Persistence.CreateDocument(newVersion);
                this._Persistence.SetParentOfContainee(newVersion, this._Persistence.GetParentIdOfContainee(updatedDocument.Id));
                this.RegisterAsNewVersion(newVersion, updatedDocument.Id);
                this.GenerateAISummaryIfAutoGenerationIsEnabled(newVersion);
                this._AuditLog.Logger.Log($"Document '{updatedDocument.Id}' updated (new version '{newVersion.Id}', version {newVersion.VersionNumber}) by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
            }
        }

        /// <summary>Generates and stores the AI-summary of the given document if the corresponding setting is enabled. Failures are logged but never propagated so that adding or updating a document is not affected by an unavailable summary-service.</summary>
        /// <param name="document">The document to summarize.</param>
        private void GenerateAISummaryIfAutoGenerationIsEnabled(Document document)
        {
            if (!this.GetAutoGenerateAISummary())
            {
                return;
            }
            try
            {
                this.GenerateAndStoreAISummary(document);
            }
            catch (Exception exception)
            {
                this._Logger.Log($"Automatic generation of the AI-summary for document '{document.Id}' failed.", exception);
            }
        }

        /// <inheritdoc />
        public void GenerateAISummary(string requesterUserId, string documentId)
        {
            this.EnsureUserIsAllowedToEditContent(requesterUserId, documentId);
            Document document = this._Persistence.GetDocument(documentId);
            this.GenerateAndStoreAISummary(document);
            this._AuditLog.Logger.Log($"AI-summary of document '{documentId}' (re)generated by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        private void GenerateAndStoreAISummary(Document document)
        {
            Model.BusinessTypes.AISummary summary = this._AISummaryService.GenerateSummary(document.Title.Value, document.OCRContent);
            document.AISummaryShort = summary.Short;
            document.AISummaryLong = summary.Long;
            this._Persistence.SetAISummary(document.Id, summary.Short, summary.Long);
            this._Logger.Log($"AI-summary for document '{document.Id}' generated.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public bool GetAutoGenerateAISummary()
        {
            string? value = this._Persistence.GetSetting(CodeUnitSpecificConstants.SettingKeyAutoGenerateAISummary);
            return value != null && bool.TryParse(value, out bool result) && result;
        }

        /// <inheritdoc />
        public void SetAutoGenerateAISummary(string requesterUserId, bool enabled)
        {
            if (!this.UserIsAdministrator(requesterUserId))
            {
                throw new NotAuthorizedException("Only administrators are allowed to change general settings.");
            }
            this._Persistence.SetSetting(CodeUnitSpecificConstants.SettingKeyAutoGenerateAISummary, enabled.ToString());
            this._AuditLog.Logger.Log($"Setting '{CodeUnitSpecificConstants.SettingKeyAutoGenerateAISummary}' set to '{enabled}' by user '{requesterUserId}'.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        private void AnalyseDocument(Document document)
        {
            this._Logger.Log($"Analyse document {document.ReadableId}", Microsoft.Extensions.Logging.LogLevel.Information);
            FileType docType;
            try
            {
                docType = SimpleOCR.Library.Core.Misc.Utilities.GetDocumentType(document.MIMEType.Value);
            }
            catch
            {
                docType = Other.Instance;
            }
            byte[]? documentAsPicture = null;
            bool toPictureWasSuccessful;
            byte[] noPreviewAvailablePicture = this._GeneralResourceLoader.GetResource("NoPreviewAvailablePicture.jpg");
            try
            {
                documentAsPicture = docType.Accept(new ToPictureVisitor(document.Content, document.MIMEType.Value));
                toPictureWasSuccessful = true;
            }
            catch
            {
                toPictureWasSuccessful = false;
            }
            try
            {
                if (toPictureWasSuccessful)
                {
                    document.Preview = this.GetPreview(documentAsPicture!);
                }
                else
                {
                    document.Preview = noPreviewAvailablePicture;
                }
            }
            catch
            {
                document.Preview = noPreviewAvailablePicture;
            }


            try
            {
                if (docType.IsBinaryFormat())
                {
                    if (toPictureWasSuccessful)
                    {
                        document.OCRContent = this._OCRService.GetOCRContent(documentAsPicture!, document.MIMEType.Value, document.AssignedLanguages);
                    }
                    else
                    {
                        document.OCRContent = string.Empty;
                    }
                }
                else
                {
                    document.OCRContent = new UTF8Encoding(false).GetString(document.Content);
                }
            }
            catch
            {
                document.OCRContent = string.Empty;
            }
        }

        private byte[] GetPreview(byte[] documentAsPicture)
        {
            if (documentAsPicture == null || documentAsPicture.Length == 0)
                throw new ArgumentException("Input image is empty.");

            using SKMemoryStream inputStream = new SKMemoryStream(documentAsPicture);
            using SKBitmap bitmap = SKBitmap.Decode(inputStream);

            int width = bitmap.Width;
            int height = bitmap.Height;

            int size = Math.Min(width, height); // Größe des Quadrats

            int cropX = 0;
            int cropY = 0;

            // Breiter als hoch → horizontal zentrieren
            if (width > height)
            {
                cropX = (width - size) / 2;
                cropY = 0;
            }
            // Höher als breit → oben behalten, unten abschneiden
            else if (height > width)
            {
                cropX = 0;
                cropY = 0; // oben bleibt
            }

            SKRectI cropRect = new SKRectI(cropX, cropY, cropX + size, cropY + size);

            using SKBitmap cropped = new SKBitmap(size, size);
            using (SKCanvas canvas = new SKCanvas(cropped))
            {
                canvas.Clear(SKColors.White);
                canvas.DrawBitmap(bitmap, cropRect, new SKRect(0, 0, size, size));
            }

            using SKImage image = SKImage.FromBitmap(cropped);
            SKData skdata = image.Encode(SKEncodedImageFormat.Png, 100);
            byte[] result = skdata.ToArray();
            return result;
        }

        /// <inheritdoc />
        public string AddStorageLocation(string requesterUserId, string name)
        {
            //any authenticated user may create a storage-location; the creator becomes its owner.
            this.EnsureAuthenticated(requesterUserId);
            string id = this._Persistence.AddStoragLocation(name);
            this._Persistence.SetOwnerOfStorageLocation(id, requesterUserId);
            this._AuditLog.Logger.Log($"Storage-location '{name}' added. (Technical-id: {id}, requester-user-id: {requesterUserId})", Microsoft.Extensions.Logging.LogLevel.Information);
            return id;
        }

        /// <inheritdoc />
        public string AddFolder(string requesterUserId, string name, string parentContainerId)
        {
            //adding a folder changes the parent-container, so the user must be allowed to change it.
            this.EnsureUserIsAllowedToEditContent(requesterUserId, parentContainerId);
            string id = this._Persistence.AddFolder(name);
            this._Persistence.SetParentOfContainee(this.GetContainee(id), parentContainerId);
            this._AuditLog.Logger.Log($"Folder '{name}' added. (Technical-id: {id}, requester-user-id: {requesterUserId})", Microsoft.Extensions.Logging.LogLevel.Information);
            return id;
        }

        /// <inheritdoc />
        public void Rename(string requesterUserId, string containerId, string newName)
        {
            this.EnsureUserIsAllowedToEditContent(requesterUserId, containerId);
            this._Persistence.Rename(containerId, newName);
            this._AuditLog.Logger.Log($"Container '{containerId}' renamed to '{newName}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public void AuthorizeUserToViewStorageLocation(string requesterUserId, string storageLocationId, string sharedWithUserId)
        {
            //only a moderator (owner) of the storage-location may manage who it is shared with.
            this.EnsureUserIsModeratorOfStorageLocation(requesterUserId, storageLocationId);
            this._Persistence.AuthorizeUserToViewStorageLocation(storageLocationId, sharedWithUserId);
            this._AuditLog.Logger.Log($"Storage-location '{storageLocationId}' shared for viewing with user '{sharedWithUserId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public void AuthorizeUserToEditStorageLocation(string requesterUserId, string storageLocationId, string editUserId)
        {
            //only a moderator (owner) of the storage-location may grant the permission to change its contents.
            this.EnsureUserIsModeratorOfStorageLocation(requesterUserId, storageLocationId);
            this._Persistence.AuthorizeUserToEditStorageLocation(storageLocationId, editUserId);
            this._AuditLog.Logger.Log($"Storage-location '{storageLocationId}' shared for editing with user '{editUserId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public void UnauthorizeUserToEditStorageLocation(string requesterUserId, string storageLocationId, string editUserId)
        {
            //only a moderator (owner) of the storage-location may revoke the permission to change its contents.
            this.EnsureUserIsModeratorOfStorageLocation(requesterUserId, storageLocationId);
            this._Persistence.UnauthorizeUserToEditStorageLocation(storageLocationId, editUserId);
            this._AuditLog.Logger.Log($"Edit-permission for storage-location '{storageLocationId}' revoked from user '{editUserId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public void UnauthorizeUserToViewStorageLocation(string requesterUserId, string storageLocationId, string sharedWithUserId)
        {
            //only a moderator (owner) of the storage-location may manage who it is shared with.
            this.EnsureUserIsModeratorOfStorageLocation(requesterUserId, storageLocationId);
            this._Persistence.UnauthorizeUserToViewStorageLocation(storageLocationId, sharedWithUserId);
            this._AuditLog.Logger.Log($"View-permission for storage-location '{storageLocationId}' revoked from user '{sharedWithUserId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public void HardDelete(string? requesterUserId, string containerOrContaineeId, string reason)
        {
            //when a user triggers the deletion, verify they may change the content. Automatic housekeeping (see issue #11) passes no requesting user and is always allowed. The check is done outside of the try-block on purpose so that a permission-error is reported to the caller instead of being swallowed by the error-logging.
            if (requesterUserId != null)
            {
                this.EnsureUserIsAllowedToEditContent(requesterUserId, containerOrContaineeId);
            }
            try
            {
                //remove from parent container. A hard-deleted document keeps its place in the containment-tree (its row is kept for traceability and it is only hidden from the listings), so only containers (folders/storage-locations) are unlinked from their parent.
                if (this._Persistence.IsContaineeId(containerOrContaineeId) && !this._Persistence.IsDocument(containerOrContaineeId))
                {
                    string parentId = this._Persistence.GetParentIdOfContainee(containerOrContaineeId);
                    this._Persistence.RemoveChild(parentId, containerOrContaineeId);
                }

                //remove content
                Core.Misc.Utilities.DoForContentObject(this._Persistence, containerOrContaineeId, (storageLocationId) => this.RemoveEntireContent(requesterUserId, storageLocationId, reason), (folderId) => this.RemoveEntireContent(requesterUserId, folderId, reason), null);

                this._Persistence.HardDelete(containerOrContaineeId);
                this._AuditLog.Logger.Log($"Hard-deleted '{containerOrContaineeId}' by {DescribeRequester(requesterUserId)}. Reason: {reason}");
            }
            catch (Exception exception)
            {
                this._Logger.Log($"Hard-deletion of '{containerOrContaineeId}' failed. Reason of the deletion-attempt: {reason}", exception);
            }
        }

        /// <inheritdoc />
        public void SoftDelete(string? requesterUserId, string containerOrContaineeId, string reason)
        {
            //when a user triggers the soft-deletion, verify they may change the content. Automatic operations pass no requesting user and are always allowed.
            if (requesterUserId != null)
            {
                this.EnsureUserIsAllowedToEditContent(requesterUserId, containerOrContaineeId);
            }

            //mark content as soft-deleted (documents are only marked, containers are handled recursively)
            Core.Misc.Utilities.DoForContentObject(this._Persistence, containerOrContaineeId,
                (storageLocationId) => this.SoftDeleteEntireContent(requesterUserId, storageLocationId, reason),
                (folderId) => this.SoftDeleteEntireContent(requesterUserId, folderId, reason),
                (documentId) => this._Persistence.SoftDelete(documentId));

            this._AuditLog.Logger.Log($"Soft-deleted '{containerOrContaineeId}' by {DescribeRequester(requesterUserId)}. Reason: {reason}");
        }

        private void SoftDeleteEntireContent(string? requesterUserId, string containerId, string reason)
        {
            IContainer container = this._Persistence.GetContainerById(containerId);
            foreach (IContainee child in container.Content)
            {
                this.SoftDelete(requesterUserId, child.Id, reason);
            }
        }

        /// <inheritdoc />
        public void Move(string requesterUserId, string containeeIdToMove, string targetContainerId)
        {
            //moving requires the permission to change both the moved containee (its current location) and the target-container it is moved into.
            this.EnsureUserIsAllowedToEditContent(requesterUserId, containeeIdToMove);
            this.EnsureUserIsAllowedToEditContent(requesterUserId, targetContainerId);
            //TODO remove containeeToMove from previous parent
            this._Persistence.SetParentOfContainee(this.GetContainee(containeeIdToMove), targetContainerId);
            this._AuditLog.Logger.Log($"Containee '{containeeIdToMove}' moved into container '{targetContainerId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        private IContainee GetContainee(string containeeId)
        {
            return Core.Misc.Utilities.DoForContentObject<IContainee>(this._Persistence, containeeId,
                (storageLocationId) => { throw new NotSupportedException(); },
                (folderId) => { return this._Persistence.GetFolder(containeeId); },
                (documentId) => { return this._Persistence.GetDocument(containeeId); }
            );
        }

        /// <inheritdoc />
        public bool UserIsAdministrator(string userId)
        {
            return this._AuthenticationService.GetUser(userId).GetAllRoles().Where(role => role.Name == CodeUnitSpecificConstants.RolenameAdmins).Any();
        }

        /// <inheritdoc />
        public IEnumerable<UserOverviewDTO> GetAllUsersWithRoles(string requesterUserId)
        {
            this.EnsureAdministrator(requesterUserId);
            ISet<GRYLibrary.Core.APIServer.CommonDBTypes.Role> allRoles = this._Persistence.GetAllRoles();
            List<UserOverviewDTO> result = new List<UserOverviewDTO>();
            foreach (Model.BusinessTypes.User user in this._Persistence.GetAllUsers().Values)
            {
                ISet<string> roleNames = allRoles.Where(role => this._Persistence.UserHasRole(user.Id, role.Id)).Select(role => role.Name).ToHashSet();
                result.Add(new UserOverviewDTO(user.Id, user.Name, roleNames));
            }
            return result;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAllRoleNames(string requesterUserId)
        {
            this.EnsureAdministrator(requesterUserId);
            return this._Persistence.GetAllRoles().Select(role => role.Name).ToList();
        }

        /// <inheritdoc />
        public void SetRolesOfUser(string requesterUserId, string targetUserId, ISet<string> roleNames)
        {
            this.EnsureAdministrator(requesterUserId);
            //resolve the requested role-names to roles first (GetRoleByName throws for an unknown role-name, so invalid input is rejected before any change is made).
            ISet<GRYLibrary.Core.APIServer.CommonDBTypes.Role> targetRoles = roleNames.Select(this._Persistence.GetRoleByName).ToHashSet();
            foreach (GRYLibrary.Core.APIServer.CommonDBTypes.Role role in this._Persistence.GetAllRoles())
            {
                bool shouldHaveRole = targetRoles.Any(targetRole => targetRole.Id == role.Id);
                bool hasRole = this._Persistence.UserHasRole(targetUserId, role.Id);
                if (shouldHaveRole && !hasRole)
                {
                    this._Persistence.AddRoleToUser(targetUserId, role.Id);
                }
                else if (!shouldHaveRole && hasRole)
                {
                    this._Persistence.RemoveRoleFromUser(targetUserId, role.Id);
                }
            }
            this._AuditLog.Logger.Log($"Roles of user '{targetUserId}' set to [{string.Join(", ", roleNames)}] by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public IEnumerable<StorageLocation> GetAllViewableStorageLocations(string requesterUserId)
        {
            List<StorageLocation> result = this._Persistence.GetAllStorageLocationIds().Where(storageLocationId => this.UserIsAllowedToViewStorageLocation(requesterUserId, storageLocationId)).Select(this._Persistence.GetStorageLocation).ToList();
            return result;
        }

        /// <inheritdoc />
        public StorageLocation GetStorageLocation(string requesterUserId, string storageLocationId)
        {
            this.EnsureUserIsAllowedToViewContent(requesterUserId, storageLocationId);
            return this._Persistence.GetStorageLocation(storageLocationId);
        }

        /// <inheritdoc />
        public Folder GetFolder(string requesterUserId, string folderId)
        {
            this.EnsureUserIsAllowedToViewContent(requesterUserId, folderId);
            return this._Persistence.GetFolder(folderId);
        }

        /// <inheritdoc />
        public void Housekeeping()
        {
            throw new NotImplementedException();//TODO remove expired accesstoken
        }

        /// <inheritdoc />
        public void RemoveEntireContent(string? requesterUserId, string containerId, string reason)
        {
            IContainer container = this._Persistence.GetContainerById(containerId);
            foreach (IContainee child in container.Content)
            {
                this.HardDelete(requesterUserId, child.Id, reason);
            }
        }

        /// <inheritdoc />
        public Document GetDocumentFromReadableId(string requesterUserId, uint readableId)
        {
            return this.GetDocument(requesterUserId, this._Persistence.GetIdFromReadableId(readableId));
        }

        /// <inheritdoc />
        public Model.BusinessTypes.User GetUser(string userId)
        {
            return this._AuthenticationService.GetUserTyped(userId);
        }

        /// <inheritdoc />
        public AccessToken Login(string username, string password)
        {
            return this._AuthenticationService.Login(username, password);
        }

    }
}
