using GRYLibrary.Core.APIServer.BaseServices;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Res;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.ExecutePrograms.WaitingStates;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.ClearScript.V8;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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
                var scriptTemplateLines = "\n".Split(this._GeneralResourceLoader.GetResourceAsString("Typescript/AdaptDocument.ts"));
                var entireScriptLines = new List<string>();
                foreach (var line in scriptTemplateLines)
                {
                    if (line.Contains("<custom-script>"))
                    {
                        entireScriptLines.AddRange(importDefinition.AdaptDocumentScriptBody.Split("\n"));
                    }
                    else if (line.Contains("<tag-definitions>"))
                    {
                        foreach (var tag in this._Persistence.GetAllTags())
                        {
                            entireScriptLines.Add($"if(name==\"{tag}\"){{return new Tag(\"{tag.Id}\", \"{tag.Name}\");}}");//TODO escape literals
                        }
                    }
                    else
                    {
                        entireScriptLines.Add(line);
                    }
                }
                entireScriptLines.AddRange(this.GetScriptPart4(document));
                using (V8ScriptEngine engine = new V8ScriptEngine())
                {
                    string typeScript = string.Join("\n", entireScriptLines);
                    string javaScript = ConvertTypeScriptToJavaScript(typeScript);
                    engine.Execute(javaScript);
                    dynamic result = engine.Script.document;
                    document.Title = result.title;
                    document.DeleteIsNotAllowedBefore = result.DeleteIsNotAllowedBefore;
                    document.MustBeHardDeletedAfter = result.MustBeHardDeletedAfter;
                    document.GroupOfBusinessOwner = result.GroupOfBusinessOwner;
                }
            }
        }

        private string ConvertTypeScriptToJavaScript(string typeScript)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            GRYLibrary.Core.Misc.Utilities.EnsureDirectoryExists(tempPath);
            try
            {
                string tsScriptFile = Path.Combine(tempPath, "script.ts");
                string jsScriptFile = Path.Combine(tempPath, "script.js");
                GRYLibrary.Core.Misc.Utilities.EnsureFileExists(tsScriptFile);
                File.WriteAllText(tsScriptFile, typeScript);

                RunTSC($"\"{tsScriptFile}\" --outFile \"{jsScriptFile}\"");

                return File.ReadAllText(jsScriptFile);
            }
            finally
            {
                GRYLibrary.Core.Misc.Utilities.EnsureDirectoryDoesNotExist(tempPath);
            }
        }

        private void RunTSC(string args)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            // Befehl und Argumente vorbereiten:
            string fileName;
            string arguments;

            if (isWindows)
            {
                fileName = "cmd.exe";
                arguments = $"/c tsc {args}";
            }
            else
            {
                fileName = "/bin/bash";
                arguments = $"-c \"tsc {args}\"";
            }
            var e = new ExternalProgramExecutor(new ExternalProgramExecutorConfiguration()
            {
                Program = fileName,
                Argument = arguments,
                WaitingState = new RunSynchronously(),
            });
            e.Run();
            if (e.ExitCode != 0)
            {
                throw new Exception($"Document-adapt-script run into an error: StdOut: {string.Join("\n", e.AllStdOutLines)}; StdErr: {string.Join("\n", e.AllStdErrLines)}");
            }
        }

        private List<string> GetScriptPart4(Document document)
        {
            var result = new List<string>();

            result.Add($@"const document = new Document({ToTSStringLiteral(document.Id)},{ToTSStringLiteral(document.Title.Value)},{ToTSStringLiteral(document.Filename.Value)},{ToTSStringLiteral(document.OriginalFilename.Value)},{ToTSDateTimeLiteral(document.ImportDate)},{ToTSTagList(document.Tags)},{ToTSIntLiteral(document.ReadableId)},{this.ToTSStringLiteral(document.MIMEType.Value)},{ToTSStringLiteral(document.OCRContent)},{ToTSDateTimeLiteral(document.DeleteIsNotAllowedBefore)},{ToTSDateTimeLiteral(document.MustBeHardDeletedAfter)},{ToTSStringLiteral(document.GroupOfBusinessOwner)},{ToTSStringLiteral(document.AddedByUserId)});new Runner(new Tools(document)).adapt();");
            return result;
        }

        private string ToTSTagList(ISet<Tag> tags)
        {
            return "[" + string.Join(", ", tags.Select(tag => $"new Tag({ToTSStringLiteral(tag.Id)},{ToTSStringLiteral(tag.Name)})")) + "]";
        }

        private string ToTSIntLiteral(ulong value)
        {
            return value.ToString();
        }

        private string ToTSDateTimeLiteral(DateTimeOffset? value)
        {
            if (value == null)
            {
                return "null";
            }
            else
            {
                return $"new Date('{value.Value:yyyy-MM-ddTHH:mm:sszzz}')";
            }
        }

        private string ToTSStringLiteral(string value)
        {
            var escaped = value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
            return $"\"{escaped}\"";
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
