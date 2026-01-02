using GRYLibrary.Core.APIServer.Services.Init;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDMSBackend.Core.Misc
{
    public class HealthCheck : IHealthCheck
    {
        private readonly IGeneralLogger _Logger;
        private readonly IPersistence _Persistence;
        private readonly IInitializationService<CommandlineParameter> _InitializationService;
        private readonly IOCRServiceClient _OCRService;
        public HealthCheck(IGeneralLogger logger, IPersistence persistence, IInitializationService<CommandlineParameter> initializationService, IOCRServiceClient ocrService)
        {
            this._Logger = logger;
            this._Persistence = persistence;
            this._InitializationService  =initializationService;
            this._OCRService = ocrService;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            this._Logger.Log("Start calculating health-status", Microsoft.Extensions.Logging.LogLevel.Debug);
            return await Tools.CheckHealthAsync(this._Logger, () =>
            {
                this._Logger.Log("Calculate health-status...", Microsoft.Extensions.Logging.LogLevel.Debug);
                IList<string> messages = new List<string>();
                HealthStatus result = HealthStatus.Healthy;

                Tools.CheckSingleExternalService(this._Logger, this._Persistence.GetType().Name, this._Persistence, ref result, messages, true, true);
                Tools.CheckSingleExternalService(this._Logger, this._OCRService.GetType().Name, this._OCRService, ref result, messages, true, false);

                return (result, messages);
            }, context, cancellationToken, this._InitializationService);
        }
    }
}
