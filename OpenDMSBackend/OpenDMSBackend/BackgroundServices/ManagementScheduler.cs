using GRYLibrary.Core.APIServer.BaseServices;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDMSBackend.Core.BackgroundServices
{
    public class ManagementScheduler : IteratingBackgroundService, IManagementScheduler
    {
        private readonly IAuditLog _AuditLog;
        private readonly IPersistence _Persistence;
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _PersistedAPIServerConfiguration;
        public ManagementScheduler(ExecutionMode executionMode, IGRYLog logger, IAuditLog auditLog, IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> persistedAPIServerConfiguration, IPersistence persistence) : base(executionMode, logger)
        {
            this.Enabled = true;
            this._AuditLog = auditLog;
            this._PersistedAPIServerConfiguration = persistedAPIServerConfiguration;
            this._Persistence = persistence;
            this.AdditionalDelay = TimeSpan.FromSeconds(2);
        }

        protected override void Run()
        {
            RunTask(DoScheduledHardDeletions, nameof(DoScheduledHardDeletions));
            RunTask(ImportNewDocuments, nameof(ImportNewDocuments));
        }

        private void RunTask(Action action, string actionName)
        {
            try
            {
                action();
            }
            catch
            {
                //TODO log exception
            }
        }

        private void DoScheduledHardDeletions()
        {
            foreach(var documentId in _Persistence.GetIdsOfDocumentsWhichMustBeHardDeletedNow())
            {
                try
                {
                    _Persistence.HardDelete(documentId);
                }
                catch (Exception exception)
                {
                    //TODO log exception
                }
            }
        }
        private void ImportNewDocuments()
        {
            foreach (var importDefinition in _PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ImportDefinitions)
            {
                try
                {
                    foreach (var document in GetDocuments(importDefinition))
                    {
                        try
                        {
                            //TODO import document
                        }
                        catch
                        {
                            //TODO log exception
                        }
                    }
                }
                catch
                {
                    //TODO log exception
                }
            }
        }

        private IEnumerable<ExternalFile> GetDocuments(ImportDefinition importDefinition)
        {
            return new List<ExternalFile>();
        }

        private class ExternalFile
        {
            public string Name { get; set; }
            public byte[] Content { get; set; }
        }
    }
}
