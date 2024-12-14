using GRYLibrary.Core.APIServer.BaseServices;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.Extensions.Logging;
using Prometheus;
using OpenDMSBackend.Core.Constants;
using System;
using OpenDMSBackend.Core.Services;

namespace OpenDMSBackend.Core.BackgroundWorker
{
    public class MetricsService : IteratingBackgroundService, IMetricsService
    {

        public Gauge MetricAmountOfDocuments { get; private set; }
        private readonly IPersistence _Persistence;
        public MetricsService(IApplicationConstants<CodeUnitSpecificConstants> constants, IGRYLog logger, IPersistence persistence) : base(constants.ExecutionMode, logger)
        {
            this.Enabled = true;
            this._Persistence = persistence;
            this.AdditionalDelay = TimeSpan.FromMinutes(1);
            this.MetricAmountOfDocuments = Metrics.CreateGauge(CodeUnitSpecificConstants.MetricsNameAmountOfDocuments, "Amount of existing documents");
        }

        public void CalculateMetrics()
        {
            try
            {
                this._Logger.Log("Calculate metrics", LogLevel.Debug);
                this.MetricAmountOfDocuments.Set(this._Persistence.GetAmountOfDocuments());
            }
            catch (Exception exception)
            {
                this._Logger.LogException(exception, "Error while calculating metrics");
            }
        }

        protected override void Run()
        {
            this.CalculateMetrics();
        }
    }
}
