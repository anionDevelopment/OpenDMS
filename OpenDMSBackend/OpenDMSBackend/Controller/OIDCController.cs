using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.DTOs;
using OpenDMSBackend.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenDMSBackend.Core.Controller
{
    [ApiController]
    [Route(ControllerRoute)]
    public class OIDCController : ControllerBase
    {
        public const string ControllerRoute = $"{ServerConfiguration.APIRoutePrefix}/v{GeneralConstants.CodeUnitMajorVersion}/{nameof(OIDCController)}";

        private readonly IGeneralLogger _Logger;
        private readonly IOIDCLoginService _OIDCLoginService;

        public OIDCController(IGeneralLogger logger, IOIDCLoginService oidcLoginService)
        {
            this._Logger = logger;
            this._OIDCLoginService = oidcLoginService;
        }

        /// <summary>Returns the list of configured OIDC providers. An empty list means OIDC login is not available.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyList<OIDCProviderDTO>))]
        [Route(nameof(GetOIDCProviders))]
        public IActionResult GetOIDCProviders()
        {
            return this.Ok(this._OIDCLoginService.GetProviders());
        }

        /// <summary>Initiates an OIDC login for the specified provider. Returns the authorization URL to redirect the user to.</summary>
        /// <param name="providerId">The id of the OIDC provider (e.g. "keycloak-1").</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OIDCInitiationDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route(nameof(InitiateOIDCLogin))]
        public async Task<IActionResult> InitiateOIDCLogin([FromQuery] string providerId)
        {
            OIDCInitiationDTO result = await this._OIDCLoginService.InitiateLoginAsync(providerId);
            return this.Ok(result);
        }

        /// <summary>
        /// Exchanges the OIDC authorization code for an OpenDMS access token.
        /// Creates a new OpenDMS account for the user on their first login via this provider.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessToken))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route(nameof(ExchangeOIDCCode))]
        public async Task<IActionResult> ExchangeOIDCCode([FromBody] OIDCCallbackRequestDTO request)
        {
            AccessToken accessToken = await this._OIDCLoginService.ExchangeCodeAsync(request.ProviderId, request.Code, request.State);
            return this.Ok(accessToken);
        }
    }
}
