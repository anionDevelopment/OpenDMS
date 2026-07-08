using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.DTOs;
using OpenDMSBackend.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace OpenDMSBackend.Core.Controller
{
    [ApiController]
    [Route(ControllerRoute)]
    public class OpenDMSBackendController : ControllerBase
    {
        public const string ControllerRoute = $"{ServerConfiguration.APIRoutePrefix}/v{GeneralConstants.CodeUnitMajorVersion}/{GeneralConstants.CodeUnitName}";

        private readonly IBusinessLogicService _BusinessLogicService;
        private readonly IAuthenticationService _AuthenticationService;
        private readonly ITimeService _TimeService;
        /// <summary>Initializes a new instance of <see cref="OpenDMSBackendController"/>.</summary>
        /// <param name="businessLogicService">The business logic service used to process document operations.</param>
        /// <param name="authenticationService">The authentication service used to resolve the current user.</param>
        /// <param name="timeService">The time service used for time-dependent operations.</param>
        public OpenDMSBackendController(IBusinessLogicService businessLogicService, IAuthenticationService authenticationService, ITimeService timeService)
        {
            this._BusinessLogicService = businessLogicService;
            this._AuthenticationService = authenticationService;
            this._TimeService = timeService;
        }

        /// <summary>Uploads a new document into the specified container.</summary>
        /// <param name="content">The raw binary content of the document.</param>
        /// <param name="containerId">The ID of the container (folder or storage location) to add the document to.</param>
        /// <param name="filename">The file name for the uploaded document.</param>
        /// <param name="title">An optional display title for the document.</param>
        /// <param name="additionalOCRLanguages">Additional languages to use during OCR processing.</param>
        /// <returns>The ID of the newly created document.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(AddDocument)}/{{{nameof(containerId)}}}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult AddDocument([FromBody] byte[] content, [FromRoute] string containerId, [FromQuery] string filename, [FromQuery] string? title, [FromQuery] IEnumerable<string> additionalOCRLanguages)
        {
            try
            {
                return this.Ok(this._BusinessLogicService.AddDocument(this.GetUser().Id, title, containerId, filename, content, this._AuthenticationService.GetBaseRoleOfAllUser(), new HashSet<string>(additionalOCRLanguages)));//TODO check if GetBaseRoleOfAllUser is really correct here
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Uploads a new version of an existing document. The new version is stored as a regular document in the same folder and is linked to the old document.</summary>
        /// <param name="content">The raw binary content of the new version.</param>
        /// <param name="oldDocumentId">The ID of the document a new version is uploaded for.</param>
        /// <param name="filename">The file name for the uploaded document.</param>
        /// <param name="title">An optional display title for the document.</param>
        /// <param name="additionalOCRLanguages">Additional languages to use during OCR processing.</param>
        /// <returns>The ID of the newly created document (the new version).</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(UploadNewVersion)}/{{{nameof(oldDocumentId)}}}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult UploadNewVersion([FromBody] byte[] content, [FromRoute] string oldDocumentId, [FromQuery] string filename, [FromQuery] string? title, [FromQuery] IEnumerable<string> additionalOCRLanguages)
        {
            return this.Ok(this._BusinessLogicService.UploadNewVersion(this.GetUser().Id, oldDocumentId, title, filename, content, this._AuthenticationService.GetBaseRoleOfAllUser(), new HashSet<string>(additionalOCRLanguages)));
        }

        /// <summary>Returns the complete version-history (from oldest to newest) of the document's version-chain.</summary>
        /// <param name="documentId">The ID of a document in the version-chain.</param>
        /// <returns>The versions as <see cref="DocumentPreviewDTO"/> objects, ordered from oldest to newest.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [ProducesResponseType(typeof(DocumentPreviewDTO[]), StatusCodes.Status200OK)]
        [Route(nameof(GetVersionHistory))]
        public IActionResult GetVersionHistory([FromQuery] string documentId)
        {
            return this.Ok(this._BusinessLogicService.GetVersionHistory(this.GetUser().Id, documentId).Select(preview => preview.ToDTO()));
        }

        /// <summary>Updates the display title of an existing document.</summary>
        /// <param name="documentId">The ID of the document whose title should be updated.</param>
        /// <param name="newTitle">The new title value to assign to the document.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [HttpPut]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(UpdateDocumentTitle)}/{{{nameof(documentId)}}}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult UpdateDocumentTitle([FromRoute] string documentId, [FromBody] StringValueDTO newTitle)
        {
            try
            {
                this._BusinessLogicService.UpdateDocumentTitle(this.GetUser().Id, documentId, newTitle.Value);
                return this.Ok();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Creates a new folder inside the specified parent folder.</summary>
        /// <param name="parentFolderId">The ID of the parent folder in which to create the new folder.</param>
        /// <param name="name">The name of the new folder.</param>
        /// <returns>The ID of the newly created folder.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(AddFolder)}/{{{nameof(parentFolderId)}}}/{{{nameof(name)}}}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult AddFolder([FromRoute] string parentFolderId, [FromRoute] string name)
        {
            return this.Ok(this._BusinessLogicService.AddFolder(this.GetUser().Id, name, parentFolderId));
        }

        /// <summary>Renames the specified container to the given new name.</summary>
        /// <param name="containerId">The id of the container to rename.</param>
        /// <param name="newName">The new name to assign.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(Rename)}/{{{nameof(containerId)}}}/{{{nameof(newName)}}}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public IActionResult Rename([FromRoute] string containerId, [FromRoute] string newName)
        {
            this._BusinessLogicService.Rename(this.GetUser().Id, containerId, newName);
            return this.Ok();
        }

        /// <summary>Moves the specified containee into the target container.</summary>
        /// <param name="containeeIdToMove">The id of the document or folder to move.</param>
        /// <param name="targetContainerId">The id of the container to move the item into.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(Move)}/{{{nameof(containeeIdToMove)}}}/{{{nameof(targetContainerId)}}}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public IActionResult Move([FromRoute] string containeeIdToMove, [FromRoute] string targetContainerId)
        {
            this._BusinessLogicService.Move(this.GetUser().Id, containeeIdToMove, targetContainerId);
            return this.Ok();
        }

        /// <summary>Grants the specified user permission to view the specified storage location.</summary>
        /// <param name="storageLocationId">The id of the storage location to share.</param>
        /// <param name="sharedWithUserId">The id of the user to grant access to.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(AuthorizeUserToViewStorageLocation)}/{{{nameof(storageLocationId)}}}/{{{nameof(sharedWithUserId)}}}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public IActionResult AuthorizeUserToViewStorageLocation([FromRoute] string storageLocationId, [FromRoute] string sharedWithUserId)
        {
            this._BusinessLogicService.AuthorizeUserToViewStorageLocation(this.GetUser().Id, storageLocationId, sharedWithUserId);
            return this.Ok();
        }

        /// <summary>Revokes the specified user's permission to view the specified storage location.</summary>
        /// <param name="storageLocationId">The id of the storage location.</param>
        /// <param name="sharedWithUserId">The id of the user whose access should be revoked.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(UnauthorizeUserToViewStorageLocation)}/{{{nameof(storageLocationId)}}}/{{{nameof(sharedWithUserId)}}}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public IActionResult UnauthorizeUserToViewStorageLocation([FromRoute] string storageLocationId, [FromRoute] string sharedWithUserId)
        {
            this._BusinessLogicService.UnauthorizeUserToViewStorageLocation(this.GetUser().Id, storageLocationId, sharedWithUserId);
            return this.Ok();
        }

        /// <summary>Grants the specified user permission to change (edit) the specified storage location and its contents. Only a moderator (owner) of the storage-location may do this.</summary>
        /// <param name="storageLocationId">The id of the storage location.</param>
        /// <param name="editUserId">The id of the user to grant the edit-permission to.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(AuthorizeUserToEditStorageLocation)}/{{{nameof(storageLocationId)}}}/{{{nameof(editUserId)}}}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public IActionResult AuthorizeUserToEditStorageLocation([FromRoute] string storageLocationId, [FromRoute] string editUserId)
        {
            this._BusinessLogicService.AuthorizeUserToEditStorageLocation(this.GetUser().Id, storageLocationId, editUserId);
            return this.Ok();
        }

        /// <summary>Revokes the specified user's permission to change (edit) the specified storage location. Only a moderator (owner) of the storage-location may do this.</summary>
        /// <param name="storageLocationId">The id of the storage location.</param>
        /// <param name="editUserId">The id of the user whose edit-permission should be revoked.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(UnauthorizeUserToEditStorageLocation)}/{{{nameof(storageLocationId)}}}/{{{nameof(editUserId)}}}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public IActionResult UnauthorizeUserToEditStorageLocation([FromRoute] string storageLocationId, [FromRoute] string editUserId)
        {
            this._BusinessLogicService.UnauthorizeUserToEditStorageLocation(this.GetUser().Id, storageLocationId, editUserId);
            return this.Ok();
        }

        /// <summary>Adds the specified user as a moderator of the specified content-object (storage-location, folder or document). Only a moderator may do this. A content-object can have several moderators.</summary>
        /// <param name="contentId">The id of the content-object.</param>
        /// <param name="newModeratorUserId">The id of the user to add as a moderator.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(AddModerator)}/{{{nameof(contentId)}}}/{{{nameof(newModeratorUserId)}}}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public IActionResult AddModerator([FromRoute] string contentId, [FromRoute] string newModeratorUserId)
        {
            this._BusinessLogicService.AddModerator(this.GetUser().Id, contentId, newModeratorUserId);
            return this.Ok();
        }

        /// <summary>Removes the specified user from the moderators of the specified content-object. Only a moderator may do this. A folder or storage-location must always keep at least one moderator.</summary>
        /// <param name="contentId">The id of the content-object.</param>
        /// <param name="moderatorUserId">The id of the moderator to remove.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(RemoveModerator)}/{{{nameof(contentId)}}}/{{{nameof(moderatorUserId)}}}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public IActionResult RemoveModerator([FromRoute] string contentId, [FromRoute] string moderatorUserId)
        {
            this._BusinessLogicService.RemoveModerator(this.GetUser().Id, contentId, moderatorUserId);
            return this.Ok();
        }

        /// <summary>Returns the ids of all moderators of the specified content-object. Only a moderator may query this.</summary>
        /// <param name="contentId">The id of the content-object.</param>
        /// <returns>The ids of the moderators.</returns>
        [Authenticate]
        [HttpGet]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(GetModerators)}/{{{nameof(contentId)}}}")]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        public IActionResult GetModerators([FromRoute] string contentId)
        {
            return this.Ok(this._BusinessLogicService.GetModerators(this.GetUser().Id, contentId));
        }

        /// <summary>Creates a new storage location with the given name owned by the current user.</summary>
        /// <param name="name">The display name of the new storage location.</param>
        /// <returns>The id of the newly created storage location.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(AddStorageLocation)}/{{{nameof(name)}}}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult AddStorageLocation([FromRoute] string name)
        {
            return this.Ok(this._BusinessLogicService.AddStorageLocation(this.GetUser().Id, name));
        }

        /// <summary>Returns the storage location with the specified id.</summary>
        /// <param name="id">The id of the storage location to retrieve.</param>
        /// <returns>The storage location as a <see cref="StorageLocationDTO"/>.</returns>
        [Authenticate]
        [HttpGet]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(GetStorageLocation)}/{{{nameof(id)}}}")]
        [ProducesResponseType(typeof(StorageLocationDTO), StatusCodes.Status200OK)]
        public IActionResult GetStorageLocation(string id)
        {
            return this.Ok(this._BusinessLogicService.GetStorageLocation(this.GetUser().Id, id).ToDTO());
        }

        /// <summary>Returns all storage locations the current user is allowed to view.</summary>
        /// <returns>An array of <see cref="StorageLocationDTO"/> representing every accessible storage location.</returns>
        [Authenticate]
        [HttpGet]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(GetAllViewableStorageLocations)}")]
        [ProducesResponseType(typeof(StorageLocationDTO[]), StatusCodes.Status200OK)]
        public IActionResult GetAllViewableStorageLocations()
        {
            return this.Ok(this._BusinessLogicService.GetAllViewableStorageLocations(this.GetUser().Id).Select(sl => sl.ToDTO()));
        }

        /// <summary>Returns the folder with the specified id.</summary>
        /// <param name="id">The id of the folder to retrieve.</param>
        /// <returns>The folder as a <see cref="FolderDTO"/>.</returns>
        [Authenticate]
        [HttpGet]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(GetFolder)}/{{{nameof(id)}}}")]
        [ProducesResponseType(typeof(FolderDTO), StatusCodes.Status200OK)]
        public IActionResult GetFolder(string id)
        {
            return this.Ok(this._BusinessLogicService.GetFolder(this.GetUser().Id, id).ToDTO());
        }

        /// <summary>Searches for documents visible to the current user that match the given search term.</summary>
        /// <param name="searchTerm">The term to search for across document titles, filenames, tags, and OCR content.</param>
        /// <returns>A list of matching documents as <see cref="DocumentPreviewDTO"/> objects, ordered by relevance.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [ProducesResponseType(typeof(DocumentPreviewDTO[]), StatusCodes.Status200OK)]
        [Route(nameof(Search))]
        public IActionResult Search([FromQuery] string searchTerm)
        {
            return this.Ok(this._BusinessLogicService.Search(this.GetUser().Id, searchTerm).Select(searchResult => searchResult.ToDTO()));
        }

        /// <summary>Returns the full document with the specified id.</summary>
        /// <param name="id">The id of the document to retrieve.</param>
        /// <returns>The document as a <see cref="DocumentDTO"/>.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [Route(nameof(GetDocument))]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status200OK)]
        public IActionResult GetDocument([FromQuery] string id)
        {
            return this.Ok(this._BusinessLogicService.GetDocument(this.GetUser().Id, id).ToDTO());
        }

        /// <summary>Returns the full document identified by its human-readable numeric id.</summary>
        /// <param name="readableId">The human-readable numeric id of the document to retrieve.</param>
        /// <returns>The document as a <see cref="DocumentDTO"/>.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [Route(nameof(GetDocumentFromReadableId))]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status200OK)]
        public IActionResult GetDocumentFromReadableId([FromQuery] uint readableId)
        {
            return this.Ok(this._BusinessLogicService.GetDocumentFromReadableId(this.GetUser().Id, readableId).ToDTO());
        }

        /// <summary>Returns a preview representation of the document with the specified id.</summary>
        /// <param name="id">The id of the document whose preview is requested.</param>
        /// <returns>The document preview as a <see cref="DocumentPreviewDTO"/>.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [Route(nameof(GetDocumentPreview))]
        [ProducesResponseType(typeof(DocumentPreviewDTO), StatusCodes.Status200OK)]
        public IActionResult GetDocumentPreview([FromQuery] string id)
        {
            return this.Ok(this._BusinessLogicService.GetDocumentPreview(this.GetUser().Id, id).ToDTO());
        }

        /// <summary>Returns the most recently added documents visible to the current user.</summary>
        /// <returns>A list of the latest documents as <see cref="DocumentPreviewDTO"/> objects.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [ProducesResponseType(typeof(DocumentPreviewDTO[]), StatusCodes.Status200OK)]
        [Route(nameof(GetLatestDocuments))]
        public IActionResult GetLatestDocuments()
        {
            return this.Ok(this._BusinessLogicService.GetLatestDocuments(this.GetUser().Id).Select(preview => preview.ToDTO()));
        }

        /// <summary>Returns all tags defined in the system.</summary>
        /// <returns>An array of all tags as <see cref="TagDTO"/> objects.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpPut]
        [ProducesResponseType(typeof(TagDTO[]), StatusCodes.Status200OK)]
        [Route(nameof(GetAllTags))]
        public IActionResult GetAllTags()
        {
            return this.Ok(this._BusinessLogicService.GetAllTags());
        }

        /// <summary>Permanently deletes the specified container or document and all its contents.</summary>
        /// <param name="containerOrContaineeId">The id of the container or document to delete.</param>
        /// <param name="reason">The stated reason for the deletion, recorded in the audit log.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpDelete]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [Route($"{nameof(HardDelete)}/{{{nameof(containerOrContaineeId)}}}")]
        public IActionResult HardDelete(string containerOrContaineeId, [FromBody] string reason)
        {
            this._BusinessLogicService.HardDelete(this.GetUser().Id, containerOrContaineeId, reason);
            return this.Ok();
        }

        /// <summary>Marks the specified container or document as soft-deleted without removing it from storage.</summary>
        /// <param name="containerOrContaineeId">The id of the container or document to soft-delete.</param>
        /// <param name="reason">The stated reason for the deletion, recorded in the audit log.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpDelete]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [Route($"{nameof(SoftDelete)}/{{{nameof(containerOrContaineeId)}}}")]
        public IActionResult SoftDelete(string containerOrContaineeId, [FromBody] string reason)
        {
            this._BusinessLogicService.SoftDelete(this.GetUser().Id, containerOrContaineeId, reason);
            return this.Ok();
        }

        /// <summary>Generates the short and the long AI-summary of the specified document and returns the updated document.</summary>
        /// <param name="documentId">The id of the document to summarize.</param>
        /// <returns>The updated document as a <see cref="DocumentDTO"/>.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpPost]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status200OK)]
        [Route($"{nameof(GenerateAISummary)}/{{{nameof(documentId)}}}")]
        public IActionResult GenerateAISummary([FromRoute] string documentId)
        {
            this._BusinessLogicService.GenerateAISummary(this.GetUser().Id, documentId);
            return this.Ok(this._BusinessLogicService.GetDocument(this.GetUser().Id, documentId).ToDTO());
        }

        /// <summary>Returns the general (admin-configurable) OpenDMS-settings.</summary>
        /// <returns>The general settings as a <see cref="GeneralSettingsDTO"/>.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpGet]
        [ProducesResponseType(typeof(GeneralSettingsDTO), StatusCodes.Status200OK)]
        [Route(nameof(GetGeneralSettings))]
        public IActionResult GetGeneralSettings()
        {
            return this.Ok(new GeneralSettingsDTO(this._BusinessLogicService.GetAutoGenerateAISummary()));
        }

        /// <summary>Updates the general (admin-configurable) OpenDMS-settings. Only administrators are allowed to call this.</summary>
        /// <param name="settings">The new settings-values.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpPut]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [Route(nameof(SetGeneralSettings))]
        public IActionResult SetGeneralSettings([FromBody] GeneralSettingsDTO settings)
        {
            this._BusinessLogicService.SetAutoGenerateAISummary(this.GetUser().Id, settings.AutoGenerateAISummary);
            return this.Ok();
        }

        /// <summary>Defines a new custom metadata-field for the specified storage-location. Only a moderator of the storage-location may do this.</summary>
        /// <param name="storageLocationId">The id of the storage-location the field is defined for.</param>
        /// <param name="field">The name and type ("String" or "Boolean") of the field to create.</param>
        /// <returns>The id of the created field-definition.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(DefineMetadataField)}/{{{nameof(storageLocationId)}}}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult DefineMetadataField([FromRoute] string storageLocationId, [FromBody] MetadataFieldDefinitionCreationDTO field)
        {
            return this.Ok(this._BusinessLogicService.DefineMetadataField(this.GetUser().Id, storageLocationId, field.Name, ParseMetadataFieldType(field.Type)));
        }

        /// <summary>Removes the specified custom metadata-field-definition together with all values documents hold for it. Only a moderator of the field's storage-location may do this.</summary>
        /// <param name="fieldDefinitionId">The id of the field-definition to remove.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [HttpDelete]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(RemoveMetadataField)}/{{{nameof(fieldDefinitionId)}}}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public IActionResult RemoveMetadataField([FromRoute] string fieldDefinitionId)
        {
            this._BusinessLogicService.RemoveMetadataField(this.GetUser().Id, fieldDefinitionId);
            return this.Ok();
        }

        /// <summary>Returns all custom metadata-fields defined for the specified storage-location. The current user must be allowed to view the storage-location.</summary>
        /// <param name="storageLocationId">The id of the storage-location.</param>
        /// <returns>The field-definitions as <see cref="MetadataFieldDefinitionDTO"/> objects.</returns>
        [Authenticate]
        [HttpGet]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(GetMetadataFields)}/{{{nameof(storageLocationId)}}}")]
        [ProducesResponseType(typeof(MetadataFieldDefinitionDTO[]), StatusCodes.Status200OK)]
        public IActionResult GetMetadataFields([FromRoute] string storageLocationId)
        {
            return this.Ok(this._BusinessLogicService.GetMetadataFields(this.GetUser().Id, storageLocationId).Select(field => field.ToDTO()));
        }

        /// <summary>Sets the value the specified document holds for the specified metadata-field. The current user must be allowed to change the document.</summary>
        /// <param name="documentId">The id of the document.</param>
        /// <param name="fieldDefinitionId">The id of the metadata-field-definition.</param>
        /// <param name="value">The value to set. For a boolean-field the value must be parseable as a boolean.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(SetDocumentMetadataValue)}/{{{nameof(documentId)}}}/{{{nameof(fieldDefinitionId)}}}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public IActionResult SetDocumentMetadataValue([FromRoute] string documentId, [FromRoute] string fieldDefinitionId, [FromBody] StringValueDTO value)
        {
            this._BusinessLogicService.SetDocumentMetadataValue(this.GetUser().Id, documentId, fieldDefinitionId, value.Value);
            return this.Ok();
        }

        /// <summary>Clears the value the specified document holds for the specified metadata-field. The current user must be allowed to change the document.</summary>
        /// <param name="documentId">The id of the document.</param>
        /// <param name="fieldDefinitionId">The id of the metadata-field-definition.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [HttpDelete]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(RemoveDocumentMetadataValue)}/{{{nameof(documentId)}}}/{{{nameof(fieldDefinitionId)}}}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public IActionResult RemoveDocumentMetadataValue([FromRoute] string documentId, [FromRoute] string fieldDefinitionId)
        {
            this._BusinessLogicService.SetDocumentMetadataValue(this.GetUser().Id, documentId, fieldDefinitionId, null);
            return this.Ok();
        }

        /// <summary>Parses the given field-type-string into a <see cref="Model.BusinessTypes.MetadataFieldType"/>, rejecting unknown values with a <see cref="GRYLibrary.Core.Exceptions.BadRequestException"/>.</summary>
        private static Model.BusinessTypes.MetadataFieldType ParseMetadataFieldType(string type)
        {
            if (System.Enum.TryParse(type, true, out Model.BusinessTypes.MetadataFieldType result) && System.Enum.IsDefined(typeof(Model.BusinessTypes.MetadataFieldType), result))
            {
                return result;
            }
            throw new GRYLibrary.Core.Exceptions.BadRequestException($"'{type}' is not a valid metadata-field-type. Allowed values are '{nameof(Model.BusinessTypes.MetadataFieldType.String)}' and '{nameof(Model.BusinessTypes.MetadataFieldType.Boolean)}'.");
        }

        private GRYLibrary.Core.APIServer.CommonDBTypes.User GetUser()
        {
            return Tools.GetUser(this.User, this._AuthenticationService);
        }
    }
}
