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
    public class CodeUnitSpecificConfiguration : ISupportRequestLoggingMiddleware, ISupportExceptionManagerMiddleware, ISupportAuthenticationMiddleware, ISupportAuthorizationMiddleware
    {
        public virtual bool RegistrationIsEnabled { get; set; }
        public virtual bool LoginIsEnabled { get; set; }
        #region Required for persistence mode
        public virtual IDatabasePersistenceConfiguration DatabasePersistenceConfiguration { get; set; }
        #endregion
        public virtual ICommonRoutesInformation CommonRoutesInformation { get; set; }
        public virtual IMaintenanceRoutesInformation MaintenanceRoutesInformation { get; set; }
        public virtual IDRequestLoggingConfiguration ConfigurationForDLoggingMiddleware { get; set; }
        public virtual IRequestLoggingConfiguration ConfigurationForLoggingMiddleware { get { return this.ConfigurationForDLoggingMiddleware; } }
        public virtual IAutSRConfiguration AuthorizationConfiguration { get; set; }
        public virtual IAuthorizationConfiguration ConfigurationForAuthorizationMiddleware { get { return this.AuthorizationConfiguration; } }
        public virtual IAuthSConfiguration AuthenticationConfiguration { get; set; }
        public virtual IAuthenticationConfiguration ConfigurationForAuthenticationMiddleware { get { return this.AuthenticationConfiguration; } }
        public virtual IExceptionManagerConfiguration ConfigurationForExceptionManagerMiddleware { get; set; }
        public virtual IHeaderServiceConfiguration HeaderServiceConfiguration { get; set; }
        public virtual IGRYLogConfiguration AuditLogConfiguration { get; set; }
        public virtual ISet<ImportDefinition> ImportDefinitions { get; set; }
        public virtual ISet<string> DefaultOCRLanguages { get; set; }
        public virtual string OCRDataFolder { get; set; }
    }
}
