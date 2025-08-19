using GRYLibrary.Core.APIServer.BaseServices;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Services;
using System;
using Microsoft.ClearScript.V8;
using System.Collections.Generic;
using GRYLibrary.Core.APIServer.Settings;

namespace OpenDMSBackend.Core.BackgroundServices
{
    public class ManagementScheduler : IteratingBackgroundService, IManagementScheduler
    {
        private readonly IAuditLog _AuditLog;
        private readonly IPersistence _Persistence;
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _PersistedAPIServerConfiguration;
        public ManagementScheduler(IGRYLog logger, IAuditLog auditLog, IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> persistedAPIServerConfiguration, IPersistence persistence,IApplicationConstants applicationConstants) : base(applicationConstants.ExecutionMode, logger)
        {
            this.Enabled = true;
            this._AuditLog = auditLog;
            this._PersistedAPIServerConfiguration = persistedAPIServerConfiguration;
            this._Persistence = persistence;
            this.AdditionalDelay = TimeSpan.FromSeconds(2);
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
                using (V8ScriptEngine engine = new V8ScriptEngine())
                {
                    string typeScript = GetSriptPart1() + importDefinition.AdaptDocumentScriptBody + GetSriptPart2()+GetScriptPaart3(document);
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
        public static string GetSriptPart1()
        {
            return $@"
class Document {{
  readonly Id: string;
  Title: string;
  Filename: string;
  readonly OriginalFilename: string;
  readonly ImportDate: Date;
  Tags: Set<Tags>;
  readonly ReadableId: bigint;
  readonly MIMEType: string;
  readonly OCRContent: string;
  DeleteIsNotAllowedBefore: string;
  MustBeHardDeletedAfter: string;
  GroupOfBusinessOwner: string;

  constructor(title: string, importDate: Date) {{
    this.Title = title;
    this.ImportDate = importDate;
  }}
}}
class Runner {{
  constructor() {{
  }}
  adapt(document:Document): Document {{
";
        }
        public static string GetSriptPart2()
        {
            return $@"
        return document;
        }};
    }}
}}
";
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

        private class ExternalFile
        {
            public string Name { get; set; }
            public byte[] Content { get; set; }
        }
    }
}
