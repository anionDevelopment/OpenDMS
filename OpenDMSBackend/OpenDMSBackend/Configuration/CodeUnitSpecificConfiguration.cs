using GRYLibrary.Core.APIServer.CommonRoutes;
using GRYLibrary.Core.APIServer.MaintenanceRoutes;
using GRYLibrary.Core.APIServer.Mid.AuthS;
using GRYLibrary.Core.APIServer.Mid.AutS;
using GRYLibrary.Core.APIServer.Mid.M05DLog;
using GRYLibrary.Core.APIServer.MidT.Aut;
using GRYLibrary.Core.APIServer.MidT.Auth;
using GRYLibrary.Core.APIServer.MidT.Exception;
using GRYLibrary.Core.APIServer.MidT.RLog;
using GRYLibrary.Core.APIServer.Services.CredH;
using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.Logging.GRYLogger;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Configuration
{
    /// <summary>Holds all code-unit-specific runtime configuration values for the OpenDMS backend.</summary>
    public class CodeUnitSpecificConfiguration : ISupportRequestLoggingMiddleware, ISupportExceptionManagerMiddleware, ISupportAuthenticationMiddleware, ISupportAuthorizationMiddleware
    {
        public bool RegistrationIsEnabled { get; set; }
        public bool LoginIsEnabled { get; set; }

        /// <summary>
        /// List of configured OpenID Connect providers.
        /// Add one entry per provider (e.g. Keycloak realm) to enable OIDC login for it.
        /// Leave empty to disable OIDC login entirely.
        /// </summary>
        public IList<OIDCProviderEntry> OIDCProviders { get; set; } = new List<OIDCProviderEntry>();
        public IDatabasePersistenceConfiguration DatabasePersistenceConfiguration { get; set; }
        public ICommonRoutesInformation CommonRoutesInformation { get; set; }
        public IMaintenanceRoutesInformation MaintenanceRoutesInformation { get; set; }
        public IDRequestLoggingConfiguration ConfigurationForDLoggingMiddleware { get; set; }
        public IRequestLoggingConfiguration ConfigurationForLoggingMiddleware { get { return this.ConfigurationForDLoggingMiddleware; } }
        public IAutSRConfiguration AuthorizationConfiguration { get; set; }
        public IAuthorizationConfiguration ConfigurationForAuthorizationMiddleware { get { return this.AuthorizationConfiguration; } }
        public IAuthSConfiguration AuthenticationConfiguration { get; set; }
        public IAuthenticationConfiguration ConfigurationForAuthenticationMiddleware { get { return this.AuthenticationConfiguration; } }
        public IExceptionManagerConfiguration ConfigurationForExceptionManagerMiddleware { get; set; }
        public IHeaderServiceConfiguration HeaderServiceConfiguration { get; set; }
        public IGRYLogConfiguration AuditLogConfiguration { get; set; }
        public IGRYLogConfiguration ManagementSchedulerServiceLogConfiguration { get; set; }
        public IGRYLogConfiguration MetricsServiceLogConfiguration { get; set; }
        public ISet<ImportDefinition> ImportDefinitions { get; set; }
        public ISet<string> DefaultOCRLanguages { get; set; }
        public string OCRDataServiceAddress { get; set; }
        public string OCRDataServiceAPIKey { get; set; }
    }
}
