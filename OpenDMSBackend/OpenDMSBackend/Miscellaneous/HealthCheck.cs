using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenDMSBackend.Core.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDMSBackend.Core.Miscellaneous
{
    public class HealthCheck : IHealthCheck
    {
        private readonly IGeneralLogger _Logger;
        private readonly IPersistence _Persistence;
        public HealthCheck(IGeneralLogger logger, IPersistence persistence)
        {
            this._Logger = logger;
            this._Persistence = persistence;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            this._Logger.Log("Start calculating health-status", Microsoft.Extensions.Logging.LogLevel.Debug);
            return await Tools.CheckHealthAsync(this._Logger, () =>
            {
                this._Logger.Log("Calculate health-status...", Microsoft.Extensions.Logging.LogLevel.Debug);
                IList<string> messages = new List<string>();
                HealthStatus result = HealthStatus.Healthy;

                Tools.CheckService(this._Logger, nameof(this._Persistence), this._Persistence, ref result, messages, true, true);
                this._Logger.Log($"{nameof(this._Persistence)} checked. Current result: {result}", Microsoft.Extensions.Logging.LogLevel.Debug);

                return (result, messages);
            }, context, cancellationToken);
        }
    }
}
