namespace OpenDMSBackend.Core.Model.DTOs
{
    /// <summary>Sent by the frontend to exchange an OIDC authorization code for an OpenDMS access token.</summary>
    public class OIDCCallbackRequestDTO
    {
        /// <summary>The provider id that initiated the flow (e.g. "keycloak-1").</summary>
        public string ProviderId { get; set; }

        /// <summary>The authorization code received from the OIDC provider callback.</summary>
        public string Code { get; set; }

        /// <summary>The state value from the callback, used to look up the stored code verifier.</summary>
        public string State { get; set; }
    }
}
