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

        private GRYLibrary.Core.APIServer.CommonDBTypes.User GetUser()
        {
            return Tools.GetUser(this.User, this._AuthenticationService);
        }
    }
}
