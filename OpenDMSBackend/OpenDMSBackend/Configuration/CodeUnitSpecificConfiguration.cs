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

        /// <summary>Base-address of the OpenAI-compatible API-endpoint which is used to generate document-summaries. May be <see langword="null"/> or empty if no summary-service is configured.</summary>
        public string? AISummaryServiceAddress { get; set; }
        /// <summary>API-key (credentials) for the OpenAI-compatible summary-service. May be <see langword="null"/> or empty if the endpoint does not require authentication.</summary>
        public string? AISummaryServiceAPIKey { get; set; }
        /// <summary>Name of the model which is used to generate document-summaries (for example "gpt-4o-mini"). May be <see langword="null"/> or empty to use the service-default.</summary>
        public string? AISummaryServiceModel { get; set; }
    }
}
