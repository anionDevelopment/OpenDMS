namespace OpenDMSBackend.Core.Model.DTOs
{
    /// <summary>Returned when initiating an OIDC login; the frontend redirects the user to <see cref="AuthorizationUrl"/>.</summary>
    public class OIDCInitiationDTO
    {
        /// <summary>The full URL to redirect the user's browser to at the OIDC provider.</summary>
        public string AuthorizationUrl { get; set; }

        /// <summary>
        /// The OIDC state value. The frontend must store this and verify it matches what comes back
        /// in the callback query parameter before calling ExchangeOIDCCode.
        /// </summary>
        public string State { get; set; }

        /// <summary>Initializes a new instance of <see cref="OIDCInitiationDTO"/>.</summary>
        /// <param name="authorizationUrl">The full URL to redirect the user's browser to at the OIDC provider.</param>
        /// <param name="state">The OIDC state value which the frontend must verify on the callback.</param>
        public OIDCInitiationDTO(string authorizationUrl, string state)
        {
            this.AuthorizationUrl = authorizationUrl;
            this.State = state;
        }
    }
}
