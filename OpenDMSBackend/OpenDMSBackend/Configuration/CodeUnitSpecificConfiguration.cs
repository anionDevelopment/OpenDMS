using GRYLibrary.Core.APIServer.Mid.M05DLog;
using GRYLibrary.Core.APIServer.MidT.Exception;
using GRYLibrary.Core.APIServer.MidT.RLog;

namespace OpenDMSBackend.Core.Configuration
{
    public class CodeUnitSpecificConfiguration : ISupportRequestLoggingMiddleware,    ISupportExceptionManagerMiddleware
    {
        public bool RegistrationIsEnabled { get; set; }
        public bool LoginIsEnabled { get; set; }
        #region Required for persistence mode
        public IDatabasePersistenceConfiguration DatabasePersistenceConfiguration { get; set; }
        #endregion
        public IExceptionManagerConfiguration ConfigurationForExceptionManagerMiddleware { get; set; }
        public IRequestLoggingConfiguration ConfigurationForLoggingMiddleware { get { return this.RequestLoggingConfiguration; } }
        public IDRequestLoggingConfiguration RequestLoggingConfiguration { get; set; }
    }
}
