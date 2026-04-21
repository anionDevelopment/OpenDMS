using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.MidT.Auth;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Misc;
using OpenDMSBackend.Core.Model.DTOs;
using OpenDMSBackend.Core.Services;
using IAuthenticationService = GRYLibrary.Core.APIServer.Services.Interfaces.IAuthenticationService;

namespace OpenDMSBackend.Core.Controller
{
    [ApiController]
    [Route(ControllerRoute)]
    public class UserController : ControllerBase
    {
        public const string ControllerRoute = $"{ServerConfiguration.APIRoutePrefix}/v{GeneralConstants.CodeUnitMajorVersion}/{nameof(UserController)}";
        private readonly IGeneralLogger _Logger;
        private readonly IPersistence _Persistence;
        private readonly IAuthenticationService _AuthenticationService;
        private readonly ITimeService _TimeService;
        private readonly IBusinessLogicService _BusinessLogicService;
        /// <summary>Initializes a new instance of <see cref="UserController"/>.</summary>
        /// <param name="logger">The logger for diagnostic output.</param>
        /// <param name="persistence">The persistence service.</param>
        /// <param name="authenticationService">The authentication service for user operations.</param>
        /// <param name="timeService">The time service.</param>
        /// <param name="businessLogicService">The business logic service.</param>
        public UserController(IGeneralLogger logger, IPersistence persistence, IAuthenticationService authenticationService, ITimeService timeService,IBusinessLogicService businessLogicService)
        {
            this._Logger = logger;
            this._Persistence = persistence;
            this._AuthenticationService = authenticationService;
            this._TimeService = timeService;
                this._BusinessLogicService = businessLogicService;
        }

        /// <summary>Authenticates a user with the given credentials and returns an access token on success.</summary>
        /// <param name="user">The username.</param>
        /// <param name="password">The plain-text password.</param>
        /// <returns>An <see cref="AccessToken"/> on success, or 401 if credentials are invalid.</returns>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccessToken))]
        [Route(nameof(Login))]
        public IActionResult Login([FromHeader] string user, [FromHeader] string password)
        {
            return this.Ok(this._AuthenticationService.Login(user, password));
        }

        /// <summary>Registers a new user account. Requires administrator privileges.</summary>
        /// <param name="user">The username for the new account.</param>
        /// <param name="password">The plain-text password for the new account.</param>
        /// <returns>200 OK on success.</returns>
        [Authenticate]
        [Authorize(CodeUnitSpecificConstants.RolenameAdmins)]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(void))]
        [Route(nameof(Register))]
        public IActionResult Register([FromHeader] string user, [FromHeader] string password)
        {
            this._BusinessLogicService.Register(user,password);
            return this.Ok();
        }

        /// <summary>Invalidates the current session's access token, logging the user out.</summary>
        /// <returns>200 OK on success, or 401 if no active token is present.</returns>
        [Authenticate]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(void))]
        [Route(nameof(Logout))]
        public IActionResult Logout()
        {
            if ((!this.HttpContext.Items.ContainsKey(AuthenticationMiddleware.CurrentlyUsedAccessTokenInformationName)) ||
                (this.HttpContext.Items[AuthenticationMiddleware.CurrentlyUsedAccessTokenInformationName] == null))
            {
                return this.StatusCode(StatusCodes.Status401Unauthorized);
            }
            string currentlyUsedAccessToken = (string)this.HttpContext.Items[AuthenticationMiddleware.CurrentlyUsedAccessTokenInformationName]!;
            this._AuthenticationService.Logout(currentlyUsedAccessToken);
            return this.Ok();
        }

        /// <summary>Checks whether the given access token is currently valid.</summary>
        /// <param name="accessToken">The access token to validate.</param>
        /// <returns><see langword="true"/> if the token is valid; otherwise <see langword="false"/>.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [Route(nameof(TokenIsValid))]
        public IActionResult TokenIsValid([FromHeader] string accessToken)
        {
            return this.Ok(this._AuthenticationService.AccessTokenIsValid(accessToken));
        }

        /// <summary>Returns the roles assigned to the currently authenticated user.</summary>
        /// <returns>An array of role names.</returns>
        [Authenticate]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string[]))]
        [Route(nameof(GetRoles))]
        public IActionResult GetRoles()
        {
            return this.Ok(this.GetUser().Roles);
        }

        /// <summary>Returns profile information for the currently authenticated user.</summary>
        /// <returns>A <see cref="UserInformationDTO"/> containing the user's id, name, and admin flag.</returns>
        [Authenticate]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserInformationDTO))]
        [Route(nameof(GetUserInformation))]
        public IActionResult GetUserInformation()
        {
            return this.Ok(Utilities.GetUserInformation(this.GetUser()));
        }

        //TODO there must be functions for an admin to grant the read-right (=>role: user) or the update-right (=>role: moderator) to certain documents only.

        private User GetUser()
        {
            return Tools.GetUser(this.User, this._AuthenticationService);
        }
    }
}
