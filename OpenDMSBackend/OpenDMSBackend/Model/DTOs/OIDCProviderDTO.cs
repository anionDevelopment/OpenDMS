namespace OpenDMSBackend.Core.Model.DTOs
{
    /// <summary>Public representation of a configured OIDC provider, returned to the frontend.</summary>
    public class OIDCProviderDTO
    {
        /// <summary>The unique provider id (e.g. "keycloak-1").</summary>
        public string Id { get; set; }

        /// <summary>The human-readable label for the login button.</summary>
        public string DisplayName { get; set; }
    }
}
