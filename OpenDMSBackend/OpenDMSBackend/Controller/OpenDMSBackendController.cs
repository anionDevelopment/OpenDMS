using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.DTOs;
using OpenDMSBackend.Core.Services;

namespace OpenDMSBackend.Core.Controller
{
    [ApiController]
    [Route(ControllerRoute)]
    public class OpenDMSBackendController : ControllerBase
    {
        public const string ControllerRoute = $"{ServerConfiguration.APIRoutePrefix}/v{GeneralConstants.CodeUnitMajorVersion}/{GeneralConstants.CodeUnitName}";

        private readonly IBusinessLogicService _BusinessLogicService;
        private readonly IAuthenticationService _AuthenticationService;
        public OpenDMSBackendController(IBusinessLogicService businessLogicService, IAuthenticationService authenticationService)
        {
            this._BusinessLogicService = businessLogicService;
            this._AuthenticationService = authenticationService;
        }

        [HttpPut]
        [Route(nameof(AddDocument))]
        public IActionResult AddDocument([FromForm] byte[] content, [FromQuery] string filename, [FromQuery] string? title)
        {
            this._BusinessLogicService.AddDocument(title, filename, content);
            return this.Ok();
        }

        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [ProducesResponseType(typeof(DocumentDTO), StatusCodes.Status200OK)]
        [Route(nameof(GetDocument))]
        public IActionResult GetDocument([FromQuery] string id)
        {
            return this.Ok(this._BusinessLogicService.GetDocument(id));
        }

        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpGet]
        [ProducesResponseType(typeof(DocumentPreviewDTO[]), StatusCodes.Status200OK)]
        [Route(nameof(Search))]
        public IActionResult Search([FromQuery] string searchTerm)
        {
            return this.Ok(this._BusinessLogicService.Search(this.GetUser().Id, searchTerm));
        }

        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpPut]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [Route($"{nameof(AssignTag)}/{{{nameof(documentId)}}}/{{{nameof(tagId)}}}")]
        public IActionResult AssignTag([FromRoute] string documentId, [FromRoute] string tagId)
        {
            this._BusinessLogicService.AssignTag(documentId, tagId);
            return this.Ok();
        }

        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameUsers)]
        [HttpPut]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [Route($"{nameof(UnassignTag)}/{{{nameof(documentId)}}}/{{{nameof(tagId)}}}")]
        public IActionResult UnassignTag([FromRoute] string documentId, [FromRoute] string tagId)
        {
            this._BusinessLogicService.UnassignTag(documentId, tagId);
            return this.Ok();
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

        private GRYLibrary.Core.APIServer.CommonDBTypes.User GetUser()
        {
            return Tools.GetUser(this.User, this._AuthenticationService);
        }
    }
}
