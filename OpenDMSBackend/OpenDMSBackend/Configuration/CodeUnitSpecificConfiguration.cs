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
using GRYLibrary.Core.Logging.GRYLogger;

namespace OpenDMSBackend.Core.Configuration
{
    public class CodeUnitSpecificConfiguration : ISupportRequestLoggingMiddleware, ISupportExceptionManagerMiddleware, ISupportAuthenticationMiddleware, ISupportAuthorizationMiddleware
    {
        public bool RegistrationIsEnabled { get; set; }
        public bool LoginIsEnabled { get; set; }
        #region Required for persistence mode
        public IDatabasePersistenceConfiguration DatabasePersistenceConfiguration { get; set; }
        #endregion
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
        public IGRYLogConfiguration AuditLogConfiguration { get;  set; }
    }
}
