using GRYLibrary.Core.APIServer.BaseServices;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Res;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.ClearScript.V8;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDMSBackend.Core.BackgroundServices
{
    public class ManagementScheduler : IteratingBackgroundService, IManagementScheduler
    {
        private readonly IAuditLog _AuditLog;
        private readonly IPersistence _Persistence;
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _PersistedAPIServerConfiguration;
        private readonly IGeneralResourceLoader _GeneralResourceLoader;
        public ManagementScheduler(IGRYLog logger, IAuditLog auditLog, IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> persistedAPIServerConfiguration, IPersistence persistence, IApplicationConstants applicationConstants, IGeneralResourceLoader generalResourceLoader) : base(applicationConstants.ExecutionMode, logger)
        {
            this.Enabled = true;
            this._GeneralResourceLoader = generalResourceLoader;
            this.AdditionalDelay = TimeSpan.FromSeconds(2);
            this._AuditLog = auditLog;
            this._PersistedAPIServerConfiguration = persistedAPIServerConfiguration;
            this._Persistence = persistence;
        }
        protected override void Run()
        {
            this.RunTask(this.DoScheduledHardDeletions, nameof(DoScheduledHardDeletions));
            this.RunTask(this.ImportNewDocuments, nameof(ImportNewDocuments));
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
            foreach (string documentId in this._Persistence.GetIdsOfDocumentsWhichMustBeHardDeletedNow())
            {
                try
                {
                    this._Persistence.HardDelete(documentId);
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
            foreach (Configuration.ImportDefinition importDefinition in this._PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ImportDefinitions)
            {
                try
                {
                    foreach (ExternalFile externalFile in this.GetDocuments(importDefinition))
                    {
                        try
                        {
                            //TODO import document
                            //TODO delete document from import source
                            Model.BusinessTypes.Document document = null;//TODO create document from externalFile
                            this._Persistence.CreateDocument(document);
                            this.RunAdaptScript(document);
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
            if (importDefinition.AdaptDocumentScriptBody != null)
            {
                var scriptTemplateLines = "\n".Split(_GeneralResourceLoader.GetResourceAsString("Typescript/document.ts"));
                var entireScriptLines = new List<string>();
                //TODO add initialization-stuff to entireScriptLines
                entireScriptLines.AddRange(GetScriptPart1(scriptTemplateLines));
                entireScriptLines.AddRange(importDefinition.AdaptDocumentScriptBody.Split("\n"));
                entireScriptLines.AddRange(GetScriptPart2(scriptTemplateLines));
                using (V8ScriptEngine engine = new V8ScriptEngine())
                {
                    string typeScript = string.Join("\n", entireScriptLines);
                    string javaScript = null;
                    engine.Execute(javaScript);
                    dynamic result = engine.Script.result;
                    document.Title = result.title;
                    document.DeleteIsNotAllowedBefore = result.DeleteIsNotAllowedBefore;
                    document.MustBeHardDeletedAfter = result.MustBeHardDeletedAfter;
                    document.GroupOfBusinessOwner = result.GroupOfBusinessOwner;
                    Console.WriteLine($"String: {result.title}");  // "HELLO"
                    Console.WriteLine($"Number: {result.resultNumber}");  // 42
                }
            }
        }

        private IList<string> GetScriptPart1(string[] scriptTemplateLines)
        {
            throw new NotImplementedException();
        }
        private IList<string> GetScriptPart2(string[] scriptTemplateLines)
        {
            throw new NotImplementedException();
        }

        public static string GetScriptPaart3(Model.BusinessTypes.Document document)
        {
            string typeScript = $@"const result = new Runner().adapt(new Document(""{document.Title}"");";//TODO pass all variables
            return typeScript;
        }

        private IEnumerable<ExternalFile> GetDocuments(OpenDMSBackend.Core.Configuration.ImportDefinition importDefinition)
        {
            return new List<ExternalFile>();
        }

        internal class ExternalFile
        {
            public string Name { get; set; }
            public byte[] Content { get; set; }
        }
    }
}
