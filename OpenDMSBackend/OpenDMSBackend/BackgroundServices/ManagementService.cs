using GRYLibrary.Core.APIServer.BaseServices;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Services.Res;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.ExecutePrograms.WaitingStates;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.Strings;
using Microsoft.ClearScript.V8;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Misc.Logger;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenDMSBackend.Core.BackgroundServices
{
    public class ManagementService : IteratingBackgroundService, IManagementScheduler
    {
        private readonly IAuditLog _AuditLog;
        private readonly IPersistence _Persistence;
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _PersistedAPIServerConfiguration;
        private readonly IGeneralResourceLoader _GeneralResourceLoader;
        private readonly IBusinessLogicService _BusinessLogicService;
        /// <summary>Initializes a new instance of <see cref="ManagementService"/>.</summary>
        /// <param name="logger">The logger used for diagnostic output.</param>
        /// <param name="auditLog">The audit-log service.</param>
        /// <param name="persistedAPIServerConfiguration">The persisted API server configuration.</param>
        /// <param name="persistence">The persistence service.</param>
        /// <param name="applicationConstants">Application-wide constants, including the execution mode.</param>
        /// <param name="generalResourceLoader">Loader for embedded general resources.</param>
        /// <param name="businessLogicService">The business-logic service used to create imported documents.</param>
        public ManagementService(IManagementServiceLog logger, IAuditLog auditLog, IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> persistedAPIServerConfiguration, IPersistence persistence, IApplicationConstants applicationConstants, IGeneralResourceLoader generalResourceLoader, IBusinessLogicService businessLogicService) : base(applicationConstants.ExecutionMode, logger.Logger)
        {
            this.Enabled = true;
            this._GeneralResourceLoader = generalResourceLoader;
            this.AdditionalDelay = TimeSpan.FromSeconds(2);
            this._AuditLog = auditLog;
            this._PersistedAPIServerConfiguration = persistedAPIServerConfiguration;
            this._Persistence = persistence;
            this._BusinessLogicService = businessLogicService;
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

        /// <summary>Performs the regulated deletion of all documents whose retention-period has ended (see issue #11): every document whose <see cref="Document.MustBeHardDeletedAfter"/> has been reached is hard-deleted in a traceable way (audit-logged with a reason via the business-logic-service). This is an automatic housekeeping-operation and therefore has no requesting user.</summary>
        internal void DoScheduledHardDeletions()
        {
            foreach (string documentId in this._Persistence.GetIdsOfDocumentsWhichMustBeHardDeletedNow())
            {
                //the business-logic-service performs the hard-deletion in a traceable way and handles/logs any deletion-error internally, so that one failing deletion does not stop the housekeeping-run.
                this._BusinessLogicService.HardDelete(null, documentId, "Regulated deletion: the retention-period ended (MustBeHardDeletedAfter reached).");
            }
        }

        internal void ImportNewDocuments()
        {
            foreach (Configuration.ImportDefinition importDefinition in this._PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.ImportDefinitions)
            {
                if (!importDefinition.IsActive)
                {
                    continue;
                }
                try
                {
                    foreach (ExternalFile externalFile in this.GetDocuments(importDefinition))
                    {
                        try
                        {
                            //the imported document has no requesting user; the AI-analysis, preview-generation and parent-assignment is done by the business-logic-service.
                            string documentId = this._BusinessLogicService.AddDocument(null, null, importDefinition.TargetFolderId, externalFile.Name, externalFile.Content, CodeUnitSpecificConstants.RolenameUsers, this.GetDefaultOCRLanguages());
                            try
                            {
                                //run the optional TypeScript adapt-script to let the import-definition mutate the metadata of the just-imported document.
                                this.ApplyAdaptScript(importDefinition, documentId);
                            }
                            catch
                            {
                                //TODO log adapt-script-exception; the document is kept with its default metadata on purpose so that the import is not retried endlessly for a broken script.
                            }
                            //remove the file from the import-source so that it is not imported again on the next iteration.
                            File.Delete(externalFile.SourcePath);
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

        private ISet<string> GetDefaultOCRLanguages()
        {
            ISet<string>? defaultOCRLanguages = this._PersistedAPIServerConfiguration.ApplicationSpecificConfiguration.DefaultOCRLanguages;
            return defaultOCRLanguages == null ? new HashSet<string>() : new HashSet<string>(defaultOCRLanguages);
        }

        /// <summary>Loads the just-imported document, runs the import-definition's TypeScript adapt-script against it and persists the resulting metadata-changes (scalar metadata via <see cref="IPersistence.Update"/> and the assigned tags via <see cref="IPersistence.AssignTag"/>).</summary>
        /// <param name="importDefinition">The import-definition that optionally carries a TypeScript script-body.</param>
        /// <param name="documentId">The id of the freshly imported document whose metadata should be adapted.</param>
        private void ApplyAdaptScript(Configuration.ImportDefinition importDefinition, string documentId)
        {
            if (importDefinition.AdaptDocumentScriptBody == null)
            {
                return;
            }
            Document document = this._Persistence.GetDocument(documentId);
            ISet<string> alreadyAssignedTagIds = document.Tags.Select(tag => tag.Id).ToHashSet();
            ISet<string> resolvedTagIds = this.RunAdaptScript(importDefinition, document);
            this._Persistence.Update(document.AddedByUserId ?? CodeUnitSpecificConstants.RolenameUsers, document);
            foreach (string tagId in resolvedTagIds.Where(tagId => !alreadyAssignedTagIds.Contains(tagId)))
            {
                this._Persistence.AssignTag(documentId, tagId);
            }
        }

        /// <summary>Runs the TypeScript adapt-script defined in the import-definition to mutate the given document's scalar metadata (title, filename, retention-dates and business-owner-group) in-memory.</summary>
        /// <param name="importDefinition">The import-definition that optionally carries a TypeScript script-body.</param>
        /// <param name="document">The document whose scalar metadata will be modified in-memory by the script.</param>
        /// <returns>The ids of the tags the script assigned to the document (to be persisted by the caller); empty if no script is defined or the script assigned no tags.</returns>
        public ISet<string> RunAdaptScript(OpenDMSBackend.Core.Configuration.ImportDefinition importDefinition, Model.BusinessTypes.Document document)
        {
            if (importDefinition.AdaptDocumentScriptBody == null)
            {
                return new HashSet<string>();
            }
            string[] scriptTemplateLines = this._GeneralResourceLoader.GetResourceAsString("Typescript.AdaptDocument.ts").Replace("\r\n", "\n").Split('\n');
            List<string> entireScriptLines = new List<string>();
            foreach (string line in scriptTemplateLines)
            {
                if (line.Contains("<custom-script>"))
                {
                    entireScriptLines.AddRange(importDefinition.AdaptDocumentScriptBody.Replace("\r\n", "\n").Split('\n'));
                }
                else if (line.Contains("<tag-definitions>"))
                {
                    foreach (Model.DTOs.TagDTO tag in this._Persistence.GetAllTags())
                    {
                        entireScriptLines.Add($"if (name === {this.ToTSStringLiteral(tag.Name)}) {{ return new Tag({this.ToTSStringLiteral(tag.Id)}, {this.ToTSStringLiteral(tag.Name)}); }}");
                    }
                }
                else
                {
                    entireScriptLines.Add(line);
                }
            }
            entireScriptLines.AddRange(this.GetScriptEpilogue(document));
            string typeScript = string.Join("\n", entireScriptLines);
            string javaScript = this.ConvertTypeScriptToJavaScript(typeScript);
            using V8ScriptEngine engine = new V8ScriptEngine();
            engine.Execute(javaScript);
            document.Title = OneLineString.From((string)engine.Script.__title);
            document.Filename = OneLineString.From((string)engine.Script.__filename);
            document.GroupOfBusinessOwner = (string)engine.Script.__groupOfBusinessOwner;
            document.DeleteIsNotAllowedBefore = this.ParseNullableDateTime(engine.Script.__deleteIsNotAllowedBefore);
            document.MustBeHardDeletedAfter = this.ParseNullableDateTime(engine.Script.__mustBeHardDeletedAfter);
            return this.ParseTagIds((string)engine.Script.__tagIds);
        }

        private ISet<string> ParseTagIds(string commaSeparatedTagIds)
        {
            if (string.IsNullOrEmpty(commaSeparatedTagIds))
            {
                return new HashSet<string>();
            }
            return commaSeparatedTagIds.Split(',').Where(tagId => !string.IsNullOrEmpty(tagId)).ToHashSet();
        }

        private DateTimeOffset? ParseNullableDateTime(object? value)
        {
            if (value == null || value is Microsoft.ClearScript.Undefined)
            {
                return null;
            }
            return DateTimeOffset.Parse((string)value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind);
        }

        private string ConvertTypeScriptToJavaScript(string typeScript)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            GRYLibrary.Core.Misc.Utilities.EnsureDirectoryExists(tempPath);
            try
            {
                string tsScriptFile = Path.Combine(tempPath, "script.ts");
                string jsScriptFile = Path.Combine(tempPath, "script.js");
                string tsConfigFile = Path.Combine(tempPath, "tsconfig.json");
                GRYLibrary.Core.Misc.Utilities.EnsureFileExists(tsScriptFile);
                File.WriteAllText(tsScriptFile, typeScript);
                //compile in an isolated project-context: a DOM-free standard-library so that the generated 'Document'-class and 'document'-variable do not collide with the built-in DOM-declarations, and empty type-roots so that no unrelated @types-packages from the surrounding file-system are picked up.
                File.WriteAllText(tsConfigFile, this.GetTSConfigContent());

                this.RunTSC("--project \"tsconfig.json\"", tempPath);

                return File.ReadAllText(jsScriptFile);
            }
            finally
            {
                GRYLibrary.Core.Misc.Utilities.EnsureDirectoryDoesNotExist(tempPath);
            }
        }

        private string GetTSConfigContent()
        {
            return @"{
  ""compilerOptions"": {
    ""target"": ""ES2020"",
    ""lib"": [""ES2020""],
    ""module"": ""commonjs"",
    ""outDir"": ""."",
    ""types"": [],
    ""typeRoots"": [],
    ""skipLibCheck"": true,
    ""noEmitOnError"": false
  },
  ""files"": [""script.ts""]
}";
        }

        private void RunTSC(string args, string workingDirectory)
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
                WorkingDirectory = workingDirectory,
                WaitingState = new RunSynchronously(),
            });
            e.Run();
            if (e.ExitCode != 0)
            {
                throw new Exception($"Document-adapt-script run into an error: StdOut: {string.Join("\n", e.AllStdOutLines)}; StdErr: {string.Join("\n", e.AllStdErrLines)}");
            }
        }

        private List<string> GetScriptEpilogue(Document document)
        {
            List<string> result = new List<string>();
            //instantiate the document with its current values, let the script adapt it and expose the (possibly changed) values as primitive global variables so that they can be read back reliably from C#.
            result.Add($"var document = new Document({this.ToTSStringLiteral(document.Id)}, {this.ToTSStringLiteral(document.Title.Value)}, {this.ToTSStringLiteral(document.Filename.Value)}, {this.ToTSStringLiteral(document.OriginalFilename.Value)}, {this.ToTSDateTimeLiteral(document.ImportDate)}, {this.ToTSTagList(document.Tags)}, {this.ToTSIntLiteral(document.ReadableId)}, {this.ToTSStringLiteral(document.MIMEType.Value)}, {this.ToTSStringLiteral(document.OCRContent ?? string.Empty)}, {this.ToTSDateTimeLiteral(document.DeleteIsNotAllowedBefore)}, {this.ToTSDateTimeLiteral(document.MustBeHardDeletedAfter)}, {this.ToTSStringLiteral(document.GroupOfBusinessOwner)}, {this.ToTSStringLiteral(document.AddedByUserId ?? string.Empty)});");
            result.Add("new Runner(new Tools()).adapt(document);");
            result.Add("var __title = document.Title;");
            result.Add("var __filename = document.Filename;");
            result.Add("var __groupOfBusinessOwner = document.GroupOfBusinessOwner;");
            result.Add("var __deleteIsNotAllowedBefore = (document.DeleteIsNotAllowedBefore === null || document.DeleteIsNotAllowedBefore === undefined) ? null : document.DeleteIsNotAllowedBefore.toISOString();");
            result.Add("var __mustBeHardDeletedAfter = (document.MustBeHardDeletedAfter === null || document.MustBeHardDeletedAfter === undefined) ? null : document.MustBeHardDeletedAfter.toISOString();");
            result.Add("var __tagIds = document.Tags.map(function (tag) { return tag.Id; }).join(\",\");");
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
            string escaped = value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
            return $"\"{escaped}\"";
        }

        /// <summary>Reads all files which are currently located in the import-source (a folder in the file-system) of the given import-definition.</summary>
        /// <param name="importDefinition">The import-definition whose <see cref="Configuration.ImportDefinition.SourceLocationURL"/> is interpreted as a file-system-folder-path.</param>
        /// <returns>The files found in the source-folder. Empty if the folder is not configured or does not exist.</returns>
        private IEnumerable<ExternalFile> GetDocuments(OpenDMSBackend.Core.Configuration.ImportDefinition importDefinition)
        {
            List<ExternalFile> result = new List<ExternalFile>();
            string? sourceFolder = importDefinition.SourceLocationURL;
            if (string.IsNullOrWhiteSpace(sourceFolder) || !Directory.Exists(sourceFolder))
            {
                return result;
            }
            foreach (string filePath in Directory.EnumerateFiles(sourceFolder))
            {
                result.Add(new ExternalFile()
                {
                    Name = Path.GetFileName(filePath),
                    Content = File.ReadAllBytes(filePath),
                    SourcePath = filePath,
                });
            }
            return result;
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
            /// <summary>The absolute path of the file in the import-source, used to remove it after a successful import.</summary>
            public string SourcePath { get; set; }
        }
    }
}
