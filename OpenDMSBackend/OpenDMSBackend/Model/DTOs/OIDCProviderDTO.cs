namespace OpenDMSBackend.Core.Model.DTOs
{
    /// <summary>Public representation of a configured OIDC provider, returned to the frontend.</summary>
    public class OIDCProviderDTO
    {
        /// <summary>The unique provider id (e.g. "keycloak-1").</summary>
        public string Id { get; set; }

        /// <summary>The human-readable label for the login button.</summary>
        public string DisplayName { get; set; }

        /// <summary>Initializes a new instance of <see cref="OIDCProviderDTO"/>.</summary>
        /// <param name="id">The unique provider id.</param>
        /// <param name="displayName">The human-readable label for the login button.</param>
        public OIDCProviderDTO(string id, string displayName)
        {
            this.Id = id;
            this.DisplayName = displayName;
        }
    }
}
