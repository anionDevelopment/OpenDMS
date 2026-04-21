using GRYLibrary.Core.APIServer.BaseServices;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.Logging.GRYLogger;
using Prometheus;
using OpenDMSBackend.Core.Constants;
using System;
using OpenDMSBackend.Core.Services;

namespace OpenDMSBackend.Core.BackgroundServices
{
    public class MetricsService : IteratingBackgroundService, IMetricsService
    {

        public Gauge MetricAmountOfDocuments { get; private set; }
        private readonly IPersistence _Persistence;
        /// <summary>Initializes a new instance of <see cref="MetricsService"/>.</summary>
        /// <param name="constants">Application-wide constants including the execution mode.</param>
        /// <param name="logger">The logger for diagnostic output.</param>
        /// <param name="persistence">The persistence service used to read document counts.</param>
        public MetricsService(IApplicationConstants<CodeUnitSpecificConstants> constants, IGRYLog logger, IPersistence persistence) : base(constants.ExecutionMode, logger)
        {
            this.Enabled = true;
            this.AdditionalDelay = TimeSpan.FromSeconds(5);
            this._Persistence = persistence;
            this.MetricAmountOfDocuments = Metrics.CreateGauge(CodeUnitSpecificConstants.MetricsNameAmountOfDocuments, "Amount of existing documents");
        }

        /// <summary>Reads the current document count from persistence and updates the Prometheus gauge.</summary>
        public void CalculateMetrics()
        {
            if (this._Persistence.IsAvailable().Item1)
            {
                try
                {
                    this.MetricAmountOfDocuments.Set(this._Persistence.GetAmountOfDocuments());
                }
                catch (Exception exception)
                {
                    this._Logger.Log("Error while calculating metrics", exception);
                }
            }
        }

        protected override void Run()
        {
            this.CalculateMetrics();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //add dispose logic here if required
            }
            base.Dispose(disposing);
        }
    }
}
