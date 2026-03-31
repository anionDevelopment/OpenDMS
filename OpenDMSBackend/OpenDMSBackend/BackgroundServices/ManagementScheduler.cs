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
                            Model.BusinessTypes.Document document = null;//TODO create document from externalFile
                            this.RunAdaptScript(document);
                            this._Persistence.CreateDocument(document);
                            //TODO delete document from import source
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
                string[] scriptTemplateLines = "\n".Split(this._GeneralResourceLoader.GetResourceAsString("Typescript/AdaptDocument.ts"));
                List<string> entireScriptLines = new List<string>();
                foreach (string line in scriptTemplateLines)
                {
                    if (line.Contains("<custom-script>"))
                    {
                        entireScriptLines.AddRange(importDefinition.AdaptDocumentScriptBody.Split("\n"));
                    }
                    else if (line.Contains("<tag-definitions>"))
                    {
                        foreach (Model.DTOs.TagDTO tag in this._Persistence.GetAllTags())
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
                string typeScript = string.Join("\n", entireScriptLines);
                string javaScript = this.ConvertTypeScriptToJavaScript(typeScript);
                using V8ScriptEngine engine = new V8ScriptEngine();
                engine.Execute(javaScript);
                dynamic result = engine.Script.document;
                document.Title = result.title;
                document.DeleteIsNotAllowedBefore = result.DeleteIsNotAllowedBefore;
                document.MustBeHardDeletedAfter = result.MustBeHardDeletedAfter;
                document.GroupOfBusinessOwner = result.GroupOfBusinessOwner;
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

                this.RunTSC($"\"{tsScriptFile}\" --outFile \"{jsScriptFile}\"");

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
            ExternalProgramExecutor e = new ExternalProgramExecutor(new ExternalProgramExecutorConfiguration()
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
            List<string> result = new List<string>();

            result.Add($@"const document = new Document({this.ToTSStringLiteral(document.Id)}, {this.ToTSStringLiteral(document.Title.Value)}, {this.ToTSStringLiteral(document.Filename.Value)}, {this.ToTSStringLiteral(document.OriginalFilename.Value)}, {this.ToTSDateTimeLiteral(document.ImportDate)},{this.ToTSTagList(document.Tags)}, {this.ToTSIntLiteral(document.ReadableId)}, {this.ToTSStringLiteral(document.MIMEType.Value)}, {this.ToTSStringLiteral(document.OCRContent)}, {this.ToTSDateTimeLiteral(document.DeleteIsNotAllowedBefore)}, {this.ToTSDateTimeLiteral(document.MustBeHardDeletedAfter)}, {this.ToTSStringLiteral(document.GroupOfBusinessOwner)}, {this.ToTSStringLiteral(document.AddedByUserId)});new Runner(new Tools(document)).adapt();");
            return result;
        }

        private string ToTSTagList(ISet<Tag> tags)
        {
            return "[" + string.Join(", ", tags.Select(tag => $"new Tag({this.ToTSStringLiteral(tag.Id)}, {this.ToTSStringLiteral(tag.Name)})")) + "]";
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
            string escaped = value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
            return $"\"{escaped}\"";
        }

        private IEnumerable<ExternalFile> GetDocuments(OpenDMSBackend.Core.Configuration.ImportDefinition importDefinition)
        {
            return new List<ExternalFile>();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //add dispose logic here if required
            }
            base.Dispose(disposing);
        }

        internal class ExternalFile
        {
            public string Name { get; set; }
            public byte[] Content { get; set; }
        }
    }
}
