using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.DTOs;
using OpenDMSBackend.Core.Services;
using System;
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
        public OpenDMSBackendController(IBusinessLogicService businessLogicService, IAuthenticationService authenticationService, ITimeService timeService)
        {
            this._BusinessLogicService = businessLogicService;
            this._AuthenticationService = authenticationService;
            this._TimeService = timeService;
        }

        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(AddDocument)}/{{{nameof(containerId)}}}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult AddDocument([FromBody] byte[] content, [FromRoute] string containerId, [FromQuery] string filename, [FromQuery] string? title)
        {
            return this.Ok(this._BusinessLogicService.AddDocument(this.GetUser().Id, title, containerId, filename, content, this._TimeService.GetCurrentTimeAsGRYDateTime(),this._AuthenticationService.GetBaseRoleOfAllUser()));
        }


        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(AddFolder)}/{{{nameof(parentFolderId)}}}/{{{nameof(name)}}}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult AddFolder([FromRoute] string parentFolderId, [FromRoute] string name)
        {
            return this.Ok(this._BusinessLogicService.AddFolder(this.GetUser().Id, name, parentFolderId));
        }

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

        [Authenticate]
        [HttpPost]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(AddStorageLocation)}/{{{nameof(name)}}}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult AddStorageLocation([FromRoute] string name)
        {
            return this.Ok(this._BusinessLogicService.AddStorageLocation(this.GetUser().Id, name));
        }

        [Authenticate]
        [HttpGet]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(GetAllViewableStorageLocations)}")]
        [ProducesResponseType(typeof(StorageLocationDTO[]), StatusCodes.Status200OK)]
        public IActionResult GetAllViewableStorageLocations()
        {
            return this.Ok(this._BusinessLogicService.GetAllViewableStorageLocations(this.GetUser().Id).Select(sl => sl.ToDTO()));
        }

        [Authenticate]
        [HttpGet]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [Route($"{nameof(GetFolder)}/{{{nameof(id)}}}")]
        [ProducesResponseType(typeof(FolderDTO), StatusCodes.Status200OK)]
        public IActionResult GetFolder(string id)
        {
            return this.Ok(this._BusinessLogicService.GetFolder(this.GetUser().Id, id).ToDTO());
        }

        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [ProducesResponseType(typeof(DocumentPreviewDTO[]), StatusCodes.Status200OK)]
        [Route(nameof(Search))]
        public IActionResult Search([FromQuery] string searchTerm)
        {
            return this.Ok(this._BusinessLogicService.Search(this.GetUser().Id, searchTerm).Select(searchResult => searchResult.ToDTO()));
        }

        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [Route(nameof(GetDocument))]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status200OK)]
        public IActionResult GetDocument([FromQuery] string id)
        {
            return this.Ok(this._BusinessLogicService.GetDocument(this.GetUser().Id, id).ToDTO());
        }

        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [Route(nameof(GetDocumentFromReadableId))]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status200OK)]
        public IActionResult GetDocumentFromReadableId([FromQuery] uint readableId)
        {
            return this.Ok(this._BusinessLogicService.GetDocumentFromReadableId(this.GetUser().Id, readableId).ToDTO());
        }

        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [Route(nameof(GetDocumentPreview))]
        [ProducesResponseType(typeof(DocumentPreviewDTO), StatusCodes.Status200OK)]
        public IActionResult GetDocumentPreview([FromQuery] string id)
        {
            return this.Ok(this._BusinessLogicService.GetDocumentPreview(this.GetUser().Id, id).ToDTO());
        }

        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [ProducesResponseType(typeof(DocumentPreviewDTO[]), StatusCodes.Status200OK)]
        [Route(nameof(GetLatestDocuments))]
        public IActionResult GetLatestDocuments()
        {
            return this.Ok(this._BusinessLogicService.GetLatestDocuments(this.GetUser().Id).Select(preview => preview.ToDTO()));
        }

        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpPut]
        [ProducesResponseType(typeof(TagDTO[]), StatusCodes.Status200OK)]
        [Route(nameof(GetAllTags))]
        public IActionResult GetAllTags()
        {
            return this.Ok(this._BusinessLogicService.GetAllTags());
        }

        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpDelete]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [Route($"{nameof(Delete)}/{{{nameof(containerOrContaineeId)}}}")]
        public IActionResult Delete(string containerOrContaineeId)
        {
            this._BusinessLogicService.Delete(this.GetUser().Id, containerOrContaineeId);
            return this.Ok();
        }

        private GRYLibrary.Core.APIServer.CommonDBTypes.User GetUser()
        {
            return Tools.GetUser(this.User, this._AuthenticationService);
        }
    }
}
