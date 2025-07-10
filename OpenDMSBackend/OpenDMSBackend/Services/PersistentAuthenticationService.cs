using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.Crypto;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using OpenDMSBackend.Core.Constants;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace OpenDMSBackend.Core.Services
{
    /// <summary>
    /// Represetns a authenticationservice where userdata (user, roles, accesstoken, etc.) will be stored persistent.
    /// </summary>
    public class OpenDMSBackendPersistentAuthenticationService : IAuthenticationService<Model.BusinessTypes.User>
    {
        private readonly IAuthenticationServicePersistence<Model.BusinessTypes.User> _AuthentificationPersistence;
        private readonly IGeneralLogger _Logger;
        private readonly IApplicationConstants<CodeUnitSpecificConstants> _Constants;
        private readonly ITimeService _TimeService;
        public OpenDMSBackendPersistentAuthenticationService(ITimeService timeService, IAuthenticationServicePersistence<Model.BusinessTypes.User> authentificationPersistence, IGeneralLogger logger, IApplicationConstants<CodeUnitSpecificConstants> constants)
        {
            this._AuthentificationPersistence = authentificationPersistence;
            this._Logger = logger;
            this._Constants = constants;
            this._TimeService = timeService;
        }

        public bool AccessTokenIsValid(string accessToken)
        {
            try
            {
                return this._AuthentificationPersistence.GetAccessToken(accessToken).IsValid(this._TimeService);
            }
            catch (KeyNotFoundException)
            {

                return false;
            }
        }

        public void AddRole(string roleName)
        {
            Role newRole = new Role()
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName,
                InheritedRoles = new HashSet<Role>(),
            };
            this._AuthentificationPersistence.AddRole(newRole);
        }

        public void AddUser(Model.BusinessTypes.User user)
        {
            this.AddUserTyped(user);
        }

        public void AddUserTyped(Model.BusinessTypes.User user)
        {
            this._AuthentificationPersistence.AddUser(user);
        }

        public void EnsureRoleDoesNotExist(string roleName)
        {
            throw new NotImplementedException();
        }

        public void EnsureRoleExists(string roleName)
        {
            if (!this.RoleExists(roleName))
            {
                this.AddRole(roleName);
            }
        }

        public void EnsureUserDoesNotHaveRole(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public void EnsureUserHasRole(string userId, string roleId)
        {
            if (!this.UserHasRole(userId, roleId))
            {
                this.AddRoleToUser(userId, roleId);
            }
        }

        public void AddRoleToUser(string userId, string roleId)
        {
            this._AuthentificationPersistence.AddRoleToUser(userId, roleId);
        }

        public ISet<Model.BusinessTypes.User> GetAllUser()
        {
            throw new NotImplementedException();
        }

        public ISet<Model.BusinessTypes.User> GetAllUserTyped()
        {
            throw new NotImplementedException();
        }

        public Role GetRoleByName(string roleName)
        {
            if (this._AuthentificationPersistence.GetAllRoles().Any())
            {
                return this._AuthentificationPersistence.GetAllRoles().Where(r => r.Name == roleName).First();
            }
            else
            {
                throw new KeyNotFoundException($"Role '{roleName}' does not exist.");
            }
        }


        public Model.BusinessTypes.User GetUserTyped(string userId)
        {
            return this._AuthentificationPersistence.GetUserById(userId);
        }

        public string Hash(string password)
        {
            string result = GUtilities.ByteArrayToHexString(new SHA256().Hash(GUtilities.StringToByteArray(password)));
            return result;
        }

        public AccessToken Login(string userName, string password)
        {
            if (!this._AuthentificationPersistence.UserWithNameExists(userName))
            {
                return this.ThrowInvalidCredentialsException();
            }

            Model.BusinessTypes.User userByNameTyped = this.GetUserByNameTyped(userName);
            if (this.Hash(password) != userByNameTyped.PasswordHash)
            {
                return this.ThrowInvalidCredentialsException();
            }

            if (userByNameTyped.UserIsLocked)
            {
                throw new NotAuthorizedException("User '" + userName + "' is locked.");
            }

            AccessToken accessToken = new AccessToken();
            accessToken.Value = Guid.NewGuid().ToString();
            accessToken.ExpiredMoment = this._TimeService.GetCurrentTime().AddDays(1.0);
            userByNameTyped.AccessToken.Add(accessToken);
            this._AuthentificationPersistence.AddAccessToken(userByNameTyped.Id, accessToken);
            return accessToken;
        }
        private AccessToken ThrowInvalidCredentialsException()
        {
            throw new BadRequestException(400, "Invalid credentials");
        }

        public void Logout(AccessToken accessToken)
        {
            throw new NotImplementedException();
        }

        public void LogoutEverywhere(string userId)
        {
            throw new NotImplementedException();
        }

        public void RemoveUser(string userId)
        {
            throw new NotImplementedException();
        }

        public bool RoleExists(string roleName)
        {
            return this._AuthentificationPersistence.RoleExists(roleName);
        }

        public bool UserExists(string userId)
        {
            return this._AuthentificationPersistence.UserWithIdExists(userId);
        }

        public bool UserHasRole(string userId, string roleId)
        {
            return this._AuthentificationPersistence.UserHasRole(userId, roleId);
        }

        public bool UserWithIdExists(string userId)
        {
            throw new NotImplementedException();
        }

        public bool UserWithNameExists(string username)
        {
            return this._AuthentificationPersistence.UserWithNameExists(username);
        }

        public void Logout(string accessToken)
        {
            this._AuthentificationPersistence.RemoveAccessToken(accessToken);
        }

        public void Logout(ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        public bool UserExistsByName(string userName)
        {
            return this._AuthentificationPersistence.UserWithNameExists(userName);
        }

        public void UpdateRole(Role role)
        {
            this._AuthentificationPersistence.UpdateRole(role);
        }

        public ISet<Role> GetRoles(Model.BusinessTypes.User user)
        {
            return user.Roles;
        }

        public void UpdateUser(Model.BusinessTypes.User user)
        {
            throw new NotImplementedException();
        }

        public Model.BusinessTypes.User GetUserByNameTyped(string userName)
        {
            return this._AuthentificationPersistence.GetUserByName(userName);
        }

        public Model.BusinessTypes.User GetUserById(string userId)
        {
            throw new NotImplementedException();
        }

        public void AddUser(User user)
        {
            this.AddUser(user as Model.BusinessTypes.User);
        }

        public string GetUserName(string accessToken)
        {
            throw new NotImplementedException();
        }

        ISet<User> IAuthenticationService.GetAllUser()
        {
            throw new NotImplementedException();
        }

        public User GetUser(string userId)
        {
            return this._AuthentificationPersistence.GetUserById(userId);
        }

        public ISet<string> GetRolesOfUser(string userId)
        {
            throw new NotImplementedException();
        }

        public User GetUserByName(string name)
        {
            throw new NotImplementedException();
        }

        public User GetUserByAccessToken(string accessToken)
        {
            return this._AuthentificationPersistence.GetUserByAccessToken(accessToken);
        }
    }
}
