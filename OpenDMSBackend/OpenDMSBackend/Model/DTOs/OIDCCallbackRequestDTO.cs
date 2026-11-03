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

        /// <summary>Initializes a new instance of <see cref="OIDCCallbackRequestDTO"/>.</summary>
        /// <param name="providerId">The provider id that initiated the flow.</param>
        /// <param name="code">The authorization code received from the OIDC provider callback.</param>
        /// <param name="state">The state value from the callback.</param>
        public OIDCCallbackRequestDTO(string providerId, string code, string state)
        {
            this.ProviderId = providerId;
            this.Code = code;
            this.State = state;
        }
    }
}
