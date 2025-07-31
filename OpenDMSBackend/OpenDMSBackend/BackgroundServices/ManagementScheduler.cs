using GRYLibrary.Core.APIServer.BaseServices;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using NLua;

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
            foreach (var documentId in _Persistence.GetIdsOfDocumentsWhichMustBeHardDeletedNow())
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

        private void RunAdaptScript(Model.BusinessTypes.Document document)
        {
        }
        private void ImportNewDocuments()
        {
            foreach (var importDefinition in _PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ImportDefinitions)
            {
                try
                {
                    foreach (var externalFile in GetDocuments(importDefinition))
                    {
                        try
                        {
                            //TODO import document
                            //TODO delete document from import source
                            Model.BusinessTypes.Document document = null;//TODO create document from externalFile
                            _Persistence.CreateDocument(document);
                            RunAdaptScript(document);
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

        public void RunAdaptScript(OpenDMSBackend.Core.Configuration.ImportDefinition importDefinition, Model.BusinessTypes.Document document)
        {
            using var lua = new Lua();
            lua.DoString(GetX1(importDefinition.AdaptDocumentScriptBody) + GetX2(document.Title.Value, document.ImportDate.ToDateTime()));

            dynamic result = lua.GetFunction("s").Call()[0];

            string newName = result["name"];
            var newImportDate = result["import_date"];
            string newBusinessOwner = result["businessowner"];

        }
        public static string GetX1(string customAdaptFunction)
        {
            var luaScript = $@"
            Document = {{}}
            Document.__index = Document

            function Document:new(name, import_date, businessowner)
                local doc = setmetatable({{}}, self)
                doc.name = name
                doc.import_date = import_date
                doc.businessowner = businessowner
                return doc
            end

            function f(document)
                ${customAdaptFunction}
            end
";
            return luaScript;
        }
        public static string GetX2(string title, DateTime importdate)
        {
            var luaScript = $@"
            function s()
                local d = Document:new('{title}', '{importdate}', 'Marketing')
                f(d)
                return d
            end
        ";
            return luaScript;
        }

        private IEnumerable<ExternalFile> GetDocuments(OpenDMSBackend.Core.Configuration.ImportDefinition importDefinition)
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
