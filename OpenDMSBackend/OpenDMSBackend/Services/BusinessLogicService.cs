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
using System.Globalization;
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
                // Reject a name which is already taken. The database enforces this as well (the unique-constraint
                // on Users.Name), but only the check here can answer with a usable error instead of letting a
                // constraint-violation surface as an internal error. It also covers the transient persistence,
                // which has no constraint at all.
                if (this._Persistence.UserWithNameExists(username))
                {
                    throw new BadRequestException($"The username '{username}' is already taken.");
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
            return this.UserHasPermissionInHierarchy(userId, contentId, RequiredPermission.Edit);
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

        /// <summary>Ensures the given user is a moderator of the content-object (or of one of its ancestors) and throws a <see cref="NotAuthorizedException"/> otherwise. Only a moderator may manage a content-object's permissions and moderators (see issue #13).</summary>
        private void EnsureUserIsModeratorOfStorageLocation(string requesterUserId, string contentId)
        {
            if (!this.UserHasPermissionInHierarchy(requesterUserId, contentId, RequiredPermission.Moderate))
            {
                throw new NotAuthorizedException($"Only a moderator of '{contentId}' may manage its permissions.");
            }
        }

        /// <inheritdoc />
        public void AddModerator(string requesterUserId, string contentId, string newModeratorUserId)
        {
            this.EnsureUserIsModeratorOfStorageLocation(requesterUserId, contentId);
            this._Persistence.SetOwnerOfStorageLocation(contentId, newModeratorUserId);
            this._AuditLog.Logger.Log($"User '{newModeratorUserId}' added as moderator of '{contentId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public void RemoveModerator(string requesterUserId, string contentId, string moderatorUserId)
        {
            this.EnsureUserIsModeratorOfStorageLocation(requesterUserId, contentId);
            //a container (storage-location or folder) must always keep at least one moderator; the last moderator can not be removed.
            if (!this._Persistence.IsDocument(contentId))
            {
                ISet<string> moderators = this._Persistence.GetOwnersOfStorageLocation(contentId);
                if (moderators.Contains(moderatorUserId) && moderators.Count <= 1)
                {
                    throw new BadRequestException($"A folder or storage-location must always have at least one moderator; the last moderator of '{contentId}' can not be removed.");
                }
            }
            this._Persistence.RemoveOwnerOfStorageLocation(contentId, moderatorUserId);
            this._AuditLog.Logger.Log($"User '{moderatorUserId}' removed as moderator of '{contentId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetModerators(string requesterUserId, string contentId)
        {
            this.EnsureUserIsModeratorOfStorageLocation(requesterUserId, contentId);
            return this._Persistence.GetOwnersOfStorageLocation(contentId);
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
                //a deleted document (soft- or hard-deleted) and an outdated version are not part of the search-result. this filter defines the search-semantics for every persistence-implementation; the sql-based implementations additionally apply it in the query itself to avoid loading previews which would be dropped here anyway.
                .Where(preview => preview.IsLatestVersion && !preview.IsHardDeleted && !preview.IsSoftDeleted)
                .ToList();
        }

        /// <inheritdoc />
        public bool UserWithNameExists(string username)
        {
            return this._Persistence.UserWithNameExists(username);
        }

        /// <inheritdoc />
        public bool UserIsAllowedToViewContent(string userId, string contentId)
        {
            return this.UserHasPermissionInHierarchy(userId, contentId, RequiredPermission.View);
        }

        /// <inheritdoc />
        public bool UserIsAllowedToViewStorageLocation(string userId, string storageLocationId)
        {
            return this.UserIsAllowedToViewContent(userId, storageLocationId);
        }

        /// <inheritdoc />
        public bool UserIsAllowedToViewFolder(string userId, string contentId)
        {
            return this.UserIsAllowedToViewContent(userId, contentId);
        }

        /// <inheritdoc />
        public bool UserIsAllowedToViewDocument(string userId, string contentId)
        {
            return this.UserIsAllowedToViewContent(userId, contentId);
        }

        /// <summary>The kind of permission required for an operation. Moderation (managing a content-object's permissions/moderators) requires being a moderator; a mere view- or edit-grant is not sufficient for it.</summary>
        private enum RequiredPermission
        {
            View,
            Edit,
            Moderate
        }

        /// <summary>Determines whether the given user has the required permission on the given content-object. Access-protection follows a default-deny concept with inheritance (see issue #13): every content-object (storage-location, folder or document) can have its own moderators ("owners") and view-/edit-grants, and a user is allowed if - at the content-object itself or at any of its ancestors up to the containing storage-location - the user is a moderator or has the required grant. Being an administrator does NOT grant access to content.</summary>
        private bool UserHasPermissionInHierarchy(string userId, string contentId, RequiredPermission required)
        {
            string currentId = contentId;
            while (true)
            {
                //a moderator ("owner") at any level of the hierarchy has all permissions on the content-object and its contents.
                if (this._Persistence.UserIsOwnerOfStorageLocation(userId, currentId))
                {
                    return true;
                }
                if (required == RequiredPermission.View && this._Persistence.StorageLocationIsSharedWithUser(currentId, userId))
                {
                    return true;
                }
                if (required == RequiredPermission.Edit && this._Persistence.StorageLocationIsEditableByUser(currentId, userId))
                {
                    return true;
                }
                if (this._Persistence.IsStorageLocation(currentId))
                {
                    //reached the root of the containment-hierarchy.
                    break;
                }
                currentId = this._Persistence.GetParentIdOfContainee(currentId);
            }
            return false;
        }

        /// <inheritdoc />
        public IEnumerable<DocumentPreview> GetLatestDocuments(string requesterUserId)
        {
            List<DocumentPreview> result = this._Persistence
                .GetAllDocumentIds()
                .Where(documentId => this.UserIsAllowedToViewContent(requesterUserId, documentId))
                .Select(id => this.GetDocumentPreview(requesterUserId, id))
                //a deleted document (soft- or hard-deleted) and an outdated version are not part of the latest-documents-list. this is the same filter the search applies.
                .Where(document => document.IsLatestVersion && !document.IsHardDeleted && !document.IsSoftDeleted)
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
            newVersion.MetadataValues = new Dictionary<string, string>(current.MetadataValues);
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

        /// <inheritdoc />
        public string GetThemeOfUser(string userId)
        {
            // A value which is not stored (or which is not valid anymore because the set of the accepted values
            // changed) is treated as the default, so that the user-interface never has to deal with an unknown value.
            string? value = this._Persistence.GetUserSetting(userId, CodeUnitSpecificConstants.UserSettingKeyTheme);
            if (value == null || !CodeUnitSpecificConstants.Themes.Contains(value))
            {
                return CodeUnitSpecificConstants.ThemeSystem;
            }
            return value;
        }

        /// <inheritdoc />
        public void SetThemeOfUser(string userId, string theme)
        {
            if (!CodeUnitSpecificConstants.Themes.Contains(theme))
            {
                throw new BadRequestException($"'{theme}' is not a valid color-scheme. Valid are: {string.Join(", ", CodeUnitSpecificConstants.Themes)}.");
            }
            this._Persistence.SetUserSetting(userId, CodeUnitSpecificConstants.UserSettingKeyTheme, theme);
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
            //every folder must have at least one moderator ("owner"); the creator becomes its first moderator.
            this._Persistence.SetOwnerOfStorageLocation(id, requesterUserId);
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
            //the retention-period is a legal requirement, so it is enforced for every caller, also for the automatic housekeeping. The check is done outside of the try-block on purpose so that the caller learns that the document was kept instead of the error being swallowed by the error-logging.
            this.EnsureRetentionPeriodAllowsHardDeletion(containerOrContaineeId);
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

        /// <summary>
        /// Ensures that no document which would be removed by hard-deleting the given content is still within its retention-period.
        /// A soft-deletion is not affected by this, because it only marks the document and keeps its content.
        /// </summary>
        private void EnsureRetentionPeriodAllowsHardDeletion(string containerOrContaineeId)
        {
            DateTimeOffset now = this._TimeService.GetCurrentLocalTimeAsDateTimeOffset();
            foreach (string documentId in this.GetContainedDocumentIds(containerOrContaineeId))
            {
                DateTimeOffset? deleteIsNotAllowedBefore = this._Persistence.GetDocumentPreview(documentId).DeleteIsNotAllowedBefore;
                if (deleteIsNotAllowedBefore.HasValue && now < deleteIsNotAllowedBefore.Value)
                {
                    throw new BadRequestException($"Document '{documentId}' must not be hard-deleted before '{deleteIsNotAllowedBefore.Value.ToString("o", CultureInfo.InvariantCulture)}' because of its retention-period.");
                }
            }
        }

        /// <inheritdoc />
        public void SetRetentionDates(string requesterUserId, string documentId, DateTimeOffset? deleteIsNotAllowedBefore, DateTimeOffset? mustBeHardDeletedAfter)
        {
            this.EnsureUserIsAllowedToEditContent(requesterUserId, documentId);
            if (deleteIsNotAllowedBefore.HasValue && mustBeHardDeletedAfter.HasValue && mustBeHardDeletedAfter.Value < deleteIsNotAllowedBefore.Value)
            {
                throw new BadRequestException("The point in time before which the document must not be deleted must not be after the point in time after which it must be deleted.");
            }
            lock (_LockObject)
            {
                Document document = this._Persistence.GetDocument(documentId);
                document.DeleteIsNotAllowedBefore = deleteIsNotAllowedBefore;
                document.MustBeHardDeletedAfter = mustBeHardDeletedAfter;
                this._Persistence.Update(requesterUserId, document);
                this._AuditLog.Logger.Log($"Retention-dates of document '{documentId}' set to '{FormatRetentionDate(deleteIsNotAllowedBefore)}' (deletion not allowed before) and '{FormatRetentionDate(mustBeHardDeletedAfter)}' (must be hard-deleted after) by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
            }
        }

        private static string FormatRetentionDate(DateTimeOffset? retentionDate)
        {
            return retentionDate.HasValue ? retentionDate.Value.ToString("o", CultureInfo.InvariantCulture) : "none";
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
            string previousStorageLocationId = this._Persistence.GetIdOfStorageLocationContainedIn(containeeIdToMove);
            //TODO remove containeeToMove from previous parent
            this._Persistence.SetParentOfContainee(this.GetContainee(containeeIdToMove), targetContainerId);
            string newStorageLocationId = this._Persistence.GetIdOfStorageLocationContainedIn(containeeIdToMove);
            if (previousStorageLocationId != newStorageLocationId)
            {
                //a metadata-field belongs to a single storage-location, so the values of every moved document have to follow into the fields of the new storage-location.
                foreach (string documentId in this.GetContainedDocumentIds(containeeIdToMove))
                {
                    this.MigrateMetadataValuesToStorageLocation(requesterUserId, documentId, previousStorageLocationId, newStorageLocationId);
                }
            }
            this._AuditLog.Logger.Log($"Containee '{containeeIdToMove}' moved into container '{targetContainerId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <summary>
        /// Transfers the metadata-values of a document which was moved into another storage-location: a value is kept when the new storage-location has a field with the same name and the same type, and is removed otherwise.
        /// Without this the value would stay stored for a field of the previous storage-location, where it is neither shown nor changeable any more.
        /// </summary>
        private void MigrateMetadataValuesToStorageLocation(string? requesterUserId, string documentId, string previousStorageLocationId, string newStorageLocationId)
        {
            IDictionary<string, string> values = this._Persistence.GetMetadataValuesOfDocument(documentId);
            if (!values.Any())
            {
                return;
            }
            IList<MetadataFieldDefinition> previousDefinitions = this._Persistence.GetMetadataFieldDefinitionsOfStorageLocation(previousStorageLocationId).ToList();
            IList<MetadataFieldDefinition> newDefinitions = this._Persistence.GetMetadataFieldDefinitionsOfStorageLocation(newStorageLocationId).ToList();
            foreach (KeyValuePair<string, string> value in values)
            {
                if (newDefinitions.Any(definition => definition.Id == value.Key))
                {
                    continue;
                }
                this._Persistence.RemoveDocumentMetadataValue(documentId, value.Key);
                MetadataFieldDefinition? previousDefinition = previousDefinitions.FirstOrDefault(definition => definition.Id == value.Key);
                MetadataFieldDefinition? matchingDefinition = previousDefinition == null
                    ? null
                    : newDefinitions.FirstOrDefault(definition => string.Equals(definition.Name, previousDefinition.Name, StringComparison.OrdinalIgnoreCase) && definition.Type == previousDefinition.Type);
                if (matchingDefinition == null)
                {
                    this._AuditLog.Logger.Log($"Metadata-value of field '{value.Key}' on document '{documentId}' removed because the document was moved into storage-location '{newStorageLocationId}' which does not have a matching field. Operation by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
                }
                else
                {
                    this._Persistence.SetDocumentMetadataValue(documentId, matchingDefinition.Id, value.Value);
                    this._AuditLog.Logger.Log($"Metadata-value of field '{value.Key}' on document '{documentId}' transferred to field '{matchingDefinition.Id}' of storage-location '{newStorageLocationId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
                }
            }
        }

        /// <summary>Returns the ids of all documents which the given containee is or contains (directly or in one of its folders).</summary>
        private IEnumerable<string> GetContainedDocumentIds(string containeeId)
        {
            return Core.Misc.Utilities.DoForContentObject<IEnumerable<string>>(this._Persistence, containeeId,
                (storageLocationId) => this.GetDocumentIdsOfContainer(storageLocationId),
                (folderId) => this.GetDocumentIdsOfContainer(folderId),
                (documentId) => new List<string>() { documentId }
            );
        }

        private IEnumerable<string> GetDocumentIdsOfContainer(string containerId)
        {
            List<string> result = new List<string>();
            foreach (IContainee child in this._Persistence.GetContainerById(containerId).Content)
            {
                result.AddRange(this.GetContainedDocumentIds(child.Id));
            }
            return result;
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
        public string DefineMetadataField(string requesterUserId, string storageLocationId, string name, MetadataFieldType type)
        {
            //custom metadata-fields are defined per storage-location and only a moderator of it may define them.
            if (!this._Persistence.IsStorageLocation(storageLocationId))
            {
                throw new BadRequestException($"Custom metadata-fields can only be defined for a storage-location, but '{storageLocationId}' is not a storage-location.");
            }
            this.EnsureUserIsModeratorOfStorageLocation(requesterUserId, storageLocationId);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new BadRequestException("The name of a metadata-field must not be empty.");
            }
            this.EnsureMetadataFieldNameIsUnused(storageLocationId, name, null);
            MetadataFieldDefinition definition = new MetadataFieldDefinition(Guid.NewGuid().ToString(), storageLocationId, name, type);
            this._Persistence.CreateMetadataFieldDefinition(definition);
            this._AuditLog.Logger.Log($"Metadata-field '{name}' (id '{definition.Id}', type {type}) defined for storage-location '{storageLocationId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
            return definition.Id;
        }

        /// <inheritdoc />
        public void RenameMetadataField(string requesterUserId, string fieldDefinitionId, string newName)
        {
            MetadataFieldDefinition definition = this._Persistence.GetMetadataFieldDefinition(fieldDefinitionId);
            this.EnsureUserIsModeratorOfStorageLocation(requesterUserId, definition.StorageLocationId);
            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new BadRequestException("The name of a metadata-field must not be empty.");
            }
            //only the name is changeable. The type stays as it is, because the values which the documents already hold for the field were validated against it.
            this.EnsureMetadataFieldNameIsUnused(definition.StorageLocationId, newName, fieldDefinitionId);
            string previousName = definition.Name;
            definition.Name = newName;
            this._Persistence.UpdateMetadataFieldDefinition(definition);
            this._AuditLog.Logger.Log($"Metadata-field '{previousName}' (id '{fieldDefinitionId}') of storage-location '{definition.StorageLocationId}' renamed to '{newName}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <summary>Ensures that no other metadata-field of the given storage-location already has the given name.</summary>
        /// <param name="storageLocationId">The id of the storage-location the field belongs to.</param>
        /// <param name="name">The name to check.</param>
        /// <param name="fieldDefinitionIdToIgnore">The id of the field which is renamed, so that keeping its own name is not reported as a conflict, or <see langword="null"/> when a field is defined.</param>
        private void EnsureMetadataFieldNameIsUnused(string storageLocationId, string name, string? fieldDefinitionIdToIgnore)
        {
            if (this._Persistence.GetMetadataFieldDefinitionsOfStorageLocation(storageLocationId).Any(existing => existing.Id != fieldDefinitionIdToIgnore && string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new BadRequestException($"A metadata-field with the name '{name}' is already defined for storage-location '{storageLocationId}'.");
            }
        }

        /// <inheritdoc />
        public void RemoveMetadataField(string requesterUserId, string fieldDefinitionId)
        {
            MetadataFieldDefinition definition = this._Persistence.GetMetadataFieldDefinition(fieldDefinitionId);
            this.EnsureUserIsModeratorOfStorageLocation(requesterUserId, definition.StorageLocationId);
            this._Persistence.DeleteMetadataFieldDefinition(fieldDefinitionId);
            this._AuditLog.Logger.Log($"Metadata-field '{definition.Name}' (id '{fieldDefinitionId}') of storage-location '{definition.StorageLocationId}' removed by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public IEnumerable<MetadataFieldDefinition> GetMetadataFields(string requesterUserId, string storageLocationId)
        {
            this.EnsureUserIsAllowedToViewContent(requesterUserId, storageLocationId);
            return this._Persistence.GetMetadataFieldDefinitionsOfStorageLocation(storageLocationId).ToList();
        }

        /// <inheritdoc />
        public IEnumerable<MetadataFieldDefinition> GetMetadataFieldsOfDocument(string requesterUserId, string documentId)
        {
            //which fields a document can hold a value for is determined by its storage-location. Resolving that here keeps the caller independent of where the document is located.
            this.EnsureUserIsAllowedToViewContent(requesterUserId, documentId);
            string storageLocationId = this._Persistence.GetIdOfStorageLocationContainedIn(documentId);
            return this._Persistence.GetMetadataFieldDefinitionsOfStorageLocation(storageLocationId).ToList();
        }

        /// <inheritdoc />
        public void SetDocumentMetadataValue(string requesterUserId, string documentId, string fieldDefinitionId, string? value)
        {
            this.EnsureUserIsAllowedToEditContent(requesterUserId, documentId);
            MetadataFieldDefinition definition = this._Persistence.GetMetadataFieldDefinition(fieldDefinitionId);
            //a metadata-field can only be set on a document contained in the storage-location the field was defined for.
            string storageLocationIdOfDocument = this._Persistence.GetIdOfStorageLocationContainedIn(documentId);
            if (definition.StorageLocationId != storageLocationIdOfDocument)
            {
                throw new BadRequestException($"The metadata-field '{fieldDefinitionId}' is not defined for the storage-location of document '{documentId}'.");
            }
            if (value == null)
            {
                this._Persistence.RemoveDocumentMetadataValue(documentId, fieldDefinitionId);
                this._AuditLog.Logger.Log($"Metadata-value of field '{fieldDefinitionId}' on document '{documentId}' cleared by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
                return;
            }
            string normalizedValue = NormalizeMetadataValue(definition, value);
            this._Persistence.SetDocumentMetadataValue(documentId, fieldDefinitionId, normalizedValue);
            this._AuditLog.Logger.Log($"Metadata-value of field '{fieldDefinitionId}' on document '{documentId}' set to '{normalizedValue}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <summary>
        /// Validates the given raw value against the field's type and returns its normalized representation.
        /// A boolean is normalized to its lower-case string-representation, a number to its round-trippable invariant-culture-representation and a timestamp to its round-trippable iso-8601-representation.
        /// Normalizing makes the stored value independent of the culture and of the format the caller used, so that every reader of the value gets the same representation back.
        /// </summary>
        private static string NormalizeMetadataValue(MetadataFieldDefinition definition, string value)
        {
            switch (definition.Type)
            {
                case MetadataFieldType.Boolean:
                    if (!bool.TryParse(value, out bool booleanValue))
                    {
                        throw new BadRequestException($"The value '{value}' is not a valid boolean-value for the metadata-field '{definition.Name}'.");
                    }
                    return booleanValue.ToString().ToLowerInvariant();
                case MetadataFieldType.Double:
                    if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue))
                    {
                        throw new BadRequestException($"The value '{value}' is not a valid number for the metadata-field '{definition.Name}'. A number must be given in the invariant culture (for example '1234.56').");
                    }
                    return doubleValue.ToString("R", CultureInfo.InvariantCulture);
                case MetadataFieldType.Timestamp:
                    if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset timestampValue))
                    {
                        throw new BadRequestException($"The value '{value}' is not a valid timestamp for the metadata-field '{definition.Name}'. A timestamp must be given in the iso-8601-format (for example '2026-01-31T12:00:00+01:00').");
                    }
                    return timestampValue.ToString("o", CultureInfo.InvariantCulture);
                case MetadataFieldType.String:
                    return value;
                default:
                    throw new BadRequestException($"Unsupported metadata-field-type '{definition.Type}'.");
            }
        }

        /// <inheritdoc />
        public string CreateTag(string requesterUserId, string tagName, ExtendedColor tagColor, bool isGlobal)
        {
            this.EnsureAuthenticated(requesterUserId);
            if (isGlobal)
            {
                //a global tag is visible for and usable by everybody, so only an administrator may create one. A tag of an ordinary user belongs to that user.
                this.EnsureAdministrator(requesterUserId);
            }
            string normalizedTagName = NormalizeTagName(tagName);
            string? ownerUserId = isGlobal ? null : requesterUserId;
            this.EnsureTagNameIsUnused(requesterUserId, normalizedTagName, null);
            Tag tag = new Tag(Guid.NewGuid().ToString(), normalizedTagName, tagColor, ownerUserId);
            this._Persistence.CreateTag(tag);
            this._AuditLog.Logger.Log($"{(isGlobal ? "Global tag" : "Tag")} '{normalizedTagName}' (id '{tag.Id}') created by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
            return tag.Id;
        }

        /// <inheritdoc />
        public TagDTO[] GetTags(string requesterUserId)
        {
            this.EnsureAuthenticated(requesterUserId);
            return this._Persistence.GetAllTags().Where(tag => tag.IsVisibleFor(requesterUserId)).Select(tag => tag.ToDTO()).ToArray();
        }

        /// <inheritdoc />
        public void UpdateTag(string requesterUserId, string tagId, string newTagName, ExtendedColor newTagColor)
        {
            Tag tag = this._Persistence.GetTag(tagId);
            this.EnsureUserIsAllowedToManageTag(requesterUserId, tag);
            string normalizedTagName = NormalizeTagName(newTagName);
            this.EnsureTagNameIsUnused(requesterUserId, normalizedTagName, tagId);
            string previousName = tag.Name;
            tag.Name = normalizedTagName;
            tag.Color = newTagColor;
            this._Persistence.UpdateTag(tag);
            this._AuditLog.Logger.Log($"Tag '{previousName}' (id '{tagId}') changed to name '{normalizedTagName}' and color '{newTagColor.GetRGBString()}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public void DeleteTag(string requesterUserId, string tagId)
        {
            Tag tag = this._Persistence.GetTag(tagId);
            this.EnsureUserIsAllowedToManageTag(requesterUserId, tag);
            //deleting the tag removes it from every document it is assigned to, because a tag-assignment without its tag would be a dangling reference.
            this._Persistence.DeleteTag(tagId);
            this._AuditLog.Logger.Log($"Tag '{tag.Name}' (id '{tagId}') deleted (including all of its assignments) by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public void AssignTag(string requesterUserId, string documentId, string tagId)
        {
            this.EnsureUserIsAllowedToEditContent(requesterUserId, documentId);
            Tag tag = this._Persistence.GetTag(tagId);
            this.EnsureTagIsVisibleForUser(requesterUserId, tag);
            if (this._Persistence.GetTagIdsOfDocument(documentId).Contains(tagId))
            {
                throw new BadRequestException($"The tag '{tag.Name}' is already assigned to document '{documentId}'.");
            }
            this._Persistence.AssignTag(documentId, tagId);
            this._AuditLog.Logger.Log($"Tag '{tag.Name}' (id '{tagId}') assigned to document '{documentId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <inheritdoc />
        public void UnassignTag(string requesterUserId, string documentId, string tagId)
        {
            this.EnsureUserIsAllowedToEditContent(requesterUserId, documentId);
            Tag tag = this._Persistence.GetTag(tagId);
            this.EnsureTagIsVisibleForUser(requesterUserId, tag);
            if (!this._Persistence.GetTagIdsOfDocument(documentId).Contains(tagId))
            {
                throw new BadRequestException($"The tag '{tag.Name}' is not assigned to document '{documentId}'.");
            }
            this._Persistence.UnassignTag(documentId, tagId);
            this._AuditLog.Logger.Log($"Tag '{tag.Name}' (id '{tagId}') unassigned from document '{documentId}' by {DescribeRequester(requesterUserId)}.", Microsoft.Extensions.Logging.LogLevel.Information);
        }

        /// <summary>Trims the given tag-name and ensures it is not empty.</summary>
        private static string NormalizeTagName(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                throw new BadRequestException("The name of a tag must not be empty.");
            }
            return tagName.Trim();
        }

        /// <summary>Ensures that no other tag which is visible for the given user already has the given name, because two tags with the same name can not be distinguished in the user-interface.</summary>
        /// <param name="requesterUserId">The id of the user the name must be unambiguous for.</param>
        /// <param name="tagName">The (already normalized) name to check.</param>
        /// <param name="tagIdToIgnore">The id of the tag which is renamed, so that keeping its own name is not reported as a conflict, or <see langword="null"/> when a tag is created.</param>
        private void EnsureTagNameIsUnused(string requesterUserId, string tagName, string? tagIdToIgnore)
        {
            if (this._Persistence.GetAllTags().Any(existing => existing.Id != tagIdToIgnore && existing.IsVisibleFor(requesterUserId) && string.Equals(existing.Name, tagName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new BadRequestException($"A tag with the name '{tagName}' already exists.");
            }
        }

        /// <summary>Ensures the given user may change or delete the given tag, which an administrator may do for a global tag and the owner may do for their own tag.</summary>
        private void EnsureUserIsAllowedToManageTag(string requesterUserId, Tag tag)
        {
            this.EnsureAuthenticated(requesterUserId);
            if (tag.OwnerUserId == null)
            {
                this.EnsureAdministrator(requesterUserId);
            }
            else if (tag.OwnerUserId != requesterUserId)
            {
                throw new NotAuthorizedException($"No permission to manage the tag '{tag.Id}' because it belongs to another user.");
            }
        }

        /// <summary>Ensures the given tag is a global tag or belongs to the given user, because a tag of another user must not be usable.</summary>
        private void EnsureTagIsVisibleForUser(string requesterUserId, Tag tag)
        {
            if (!tag.IsVisibleFor(requesterUserId))
            {
                throw new NotAuthorizedException($"No permission to use the tag '{tag.Id}' because it belongs to another user.");
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
