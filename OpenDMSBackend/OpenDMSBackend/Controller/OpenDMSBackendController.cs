using GRYLibrary.Core.APIServer.Settings.Configuration;
using Microsoft.AspNetCore.Mvc;
using OpenDMSBackend.Core.Constants;

namespace OpenDMSBackend.Core.Controller
{
    [ApiController]
    [Route(ControllerRoute)]
    public class OpenDMSBackendController : ControllerBase
    {
        public const string ControllerRoute = $"{ServerConfiguration.APIRoutePrefix}/v{GeneralConstants.CodeUnitMajorVersion}/{GeneralConstants.CodeUnitName}";
        [HttpGet]
        [Route(nameof(Register))]
        public IActionResult Register()
        {
          throw new System.NotImplementedException();
        }

    }
}
