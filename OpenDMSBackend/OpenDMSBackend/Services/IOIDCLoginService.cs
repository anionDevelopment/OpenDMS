using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using OpenDMSBackend.Core.Model.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenDMSBackend.Core.Services
{
    /// <summary>Application-level service that orchestrates OIDC login and OpenDMS user provisioning.</summary>
    public interface IOIDCLoginService
    {
        /// <summary>Returns the publicly visible list of configured OIDC providers.</summary>
        IReadOnlyList<OIDCProviderDTO> GetProviders();

        /// <summary>
        /// Initiates an OIDC login for the given provider.
        /// Returns the authorization URL to redirect the user to, plus the state value for CSRF protection.
        /// </summary>
        Task<OIDCInitiationDTO> InitiateLoginAsync(string providerId);

        /// <summary>
        /// Exchanges the OIDC authorization code for an OpenDMS access token.
        /// Creates a new OpenDMS user on first login if none exists for this external account.
        /// </summary>
        Task<AccessToken> ExchangeCodeAsync(string providerId, string code, string state);
    }
}
