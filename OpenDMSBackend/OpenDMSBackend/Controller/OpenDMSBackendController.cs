using GRYLibrary.Core.APIServer.Settings.Configuration;
using Microsoft.AspNetCore.Mvc;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.ServiceInterfaces;

namespace OpenDMSBackend.Core.Controller
{
    [ApiController]
    [Route(ControllerRoute)]
    public class OpenDMSBackendController : ControllerBase
    {
        public const string ControllerRoute = $"{ServerConfiguration.APIRoutePrefix}/v{GeneralConstants.CodeUnitMajorVersion}/{GeneralConstants.CodeUnitName}";

        private readonly IBusinessLogicService _BusinessLogicService;
        public OpenDMSBackendController(IBusinessLogicService businessLogicService)
        {
            this._BusinessLogicService = businessLogicService;
        }

        [HttpGet]
        [Route(nameof(Register))]
        public IActionResult Register([FromHeader]string username, [FromHeader] string password)
        {
            this._BusinessLogicService.Register(username, password);
            return this.Ok();
        }

        [HttpPut]
        [Route(nameof(AddDocument))]
        public IActionResult AddDocument([FromForm] byte[] content, [FromQuery] string filename, [FromQuery] string? title)
        {
            this._BusinessLogicService.AddDocument(title, filename, content);
            return this.Ok();
        }

        [HttpGet]
        [Route(nameof(GetDocument))]
        public IActionResult GetDocument(string id)
        {
          return this.Ok(this._BusinessLogicService.GetDocument(id));
        }

        [HttpGet]
        [Route(nameof(Search))]
        public IActionResult Search(string searchTerm)
        {
            return this.Ok(this._BusinessLogicService.Search(searchTerm));
        }

    }
}
