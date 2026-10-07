using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.APIServer.Services.OIDC;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.DTOs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenDMSBackend.Core.Services
{
    /// <summary>
    /// Orchestrates the OIDC Authorization Code flow with PKCE and provisions OpenDMS users on first login.
    /// Uses <see cref="IOIDCService"/> from GRYLibrary for the generic OIDC protocol work.
    /// </summary>
    public class OIDCLoginService : IOIDCLoginService
    {
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _Configuration;
        private readonly IOIDCService _OIDCService;
        private readonly IPersistence _Persistence;
        private readonly IAuthenticationService<Model.BusinessTypes.User> _AuthenticationService;
        private readonly ITimeService _TimeService;

        private readonly ConcurrentDictionary<string /*state*/, PendingOIDCLogin> _PendingLogins = new();

        private record PendingOIDCLogin(string ProviderId, string CodeVerifier, DateTimeOffset CreatedAt);

        public OIDCLoginService(
            IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration,
            IOIDCService oidcService,
            IPersistence persistence,
            IAuthenticationService<Model.BusinessTypes.User> authenticationService,
            ITimeService timeService)
        {
            this._Configuration = configuration;
            this._OIDCService = oidcService;
            this._Persistence = persistence;
            this._AuthenticationService = authenticationService;
            this._TimeService = timeService;
        }

        /// <inheritdoc/>
        public IReadOnlyList<OIDCProviderDTO> GetProviders()
        {
            return this._Configuration.ApplicationSpecificConfiguration.OIDCProviders
                .Select(p => new OIDCProviderDTO { Id = p.Id, DisplayName = p.DisplayName })
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<OIDCInitiationDTO> InitiateLoginAsync(string providerId)
        {
            OIDCProviderConfiguration providerConfig = this.GetProviderConfig(providerId);
            OIDCAuthorizationRequest request = await this._OIDCService.InitiateLoginAsync(providerConfig);

            this.CleanupExpiredLogins();
            this._PendingLogins[request.State] = new PendingOIDCLogin(providerId, request.CodeVerifier, this._TimeService.GetCurrentLocalTimeAsDateTimeOffset());

            return new OIDCInitiationDTO
            {
                AuthorizationUrl = request.AuthorizationUrl,
                State = request.State,
            };
        }

        /// <inheritdoc/>
        public async Task<AccessToken> ExchangeCodeAsync(string providerId, string code, string state)
        {
            if (!this._PendingLogins.TryRemove(state, out PendingOIDCLogin? pending))
            {
                throw new BadRequestException(400, "Invalid or expired OIDC state. Please restart the login process.");
            }

            if (pending.ProviderId != providerId)
            {
                throw new BadRequestException(400, "Provider mismatch in OIDC callback.");
            }

            OIDCProviderConfiguration providerConfig = this.GetProviderConfig(providerId);
            OIDCTokenResult tokenResult = await this._OIDCService.ExchangeCodeAsync(providerConfig, code, pending.CodeVerifier);

            Model.BusinessTypes.User user = this.FindOrCreateUser(providerId, tokenResult);

            AccessToken accessToken = new AccessToken
            {
                Value = Guid.NewGuid().ToString(),
                ExpiredMoment = this._TimeService.GetCurrentLocalTimeAsDateTimeOffset().AddDays(1.0),
                OwnerUserId = user.Id,
            };
            user.AccessToken.Add(accessToken);
            this._Persistence.AddAccessToken(accessToken);

            return accessToken;
        }

        private Model.BusinessTypes.User FindOrCreateUser(string providerId, OIDCTokenResult tokenResult)
        {
            Model.BusinessTypes.User? existingUser = this._Persistence.GetUserByExternalLogin(providerId, tokenResult.Subject);
            if (existingUser != null)
            {
                return existingUser;
            }

            string username = this.DeriveUsername(providerId, tokenResult);
            Model.BusinessTypes.User newUser = Model.BusinessTypes.User.CreateExternalUser(username, providerId, tokenResult.Subject, this._TimeService);
            newUser.EMailAddress = tokenResult.Email;

            this._AuthenticationService.AddUserTyped(newUser);

            GRYLibrary.Core.APIServer.CommonDBTypes.Role userRole = this._AuthenticationService.GetRoleByName(CodeUnitSpecificConstants.RolenameUsers);
            this._AuthenticationService.EnsureUserHasRole(newUser.Id, userRole.Id);

            return newUser;
        }

        private string DeriveUsername(string providerId, OIDCTokenResult tokenResult)
        {
            string baseName = tokenResult.PreferredUsername ?? tokenResult.Email ?? tokenResult.Subject;
            string candidate = $"{baseName}@{providerId}";

            if (!this._Persistence.UserWithNameExists(candidate))
            {
                return candidate;
            }

            return $"{tokenResult.Subject}@{providerId}";
        }

        private OIDCProviderConfiguration GetProviderConfig(string providerId)
        {
            OIDCProviderEntry? entry = this._Configuration.ApplicationSpecificConfiguration.OIDCProviders
                .FirstOrDefault(p => p.Id == providerId);
            if (entry == null)
            {
                throw new KeyNotFoundException($"No OIDC provider configured with id '{providerId}'.");
            }
            return new OIDCProviderConfiguration
            {
                Id = entry.Id,
                DisplayName = entry.DisplayName,
                Authority = entry.Authority,
                ClientId = entry.ClientId,
                RedirectUri = entry.RedirectUri,
            };
        }

        private void CleanupExpiredLogins()
        {
            DateTimeOffset cutoff = this._TimeService.GetCurrentLocalTimeAsDateTimeOffset().AddMinutes(-10);
            foreach (KeyValuePair<string, PendingOIDCLogin> kvp in this._PendingLogins.ToArray())
            {
                if (kvp.Value.CreatedAt < cutoff)
                {
                    this._PendingLogins.TryRemove(kvp.Key, out _);
                }
            }
        }
    }
}
