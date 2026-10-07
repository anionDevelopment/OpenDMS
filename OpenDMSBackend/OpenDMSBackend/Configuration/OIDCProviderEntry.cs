namespace OpenDMSBackend.Core.Configuration
{
    /// <summary>
    /// Configuration entry for one OpenID Connect provider.
    /// Add one entry per Keycloak realm (or any other OIDC-compatible provider) you want to enable.
    /// </summary>
    public class OIDCProviderEntry
    {
        /// <summary>
        /// Unique identifier for this provider within OpenDMS (e.g. "keycloak-1").
        /// Referenced internally and passed back to the frontend.
        /// </summary>
        public string Id { get; set; }

        /// <summary>Label shown on the login button (e.g. "Company Keycloak").</summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// OIDC authority / issuer base URL.
        /// Example for Keycloak: "https://keycloak.example.com/realms/myrealm"
        /// </summary>
        public string Authority { get; set; }

        /// <summary>The client_id registered in the OIDC provider for OpenDMS.</summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Redirect URI that receives the authorization code after the user logs in.
        /// Must match what is configured in the OIDC provider client settings.
        /// Should be the OpenDMS frontend callback URL, e.g. "https://opendms.example.com/oidc-callback".
        /// </summary>
        public string RedirectUri { get; set; }
    }
}
