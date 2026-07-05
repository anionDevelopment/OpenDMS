using GRYLibrary.Core.APIServer;
using GRYLibrary.Core.APIServer.ExecutionModes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Res;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackend.Core.BackgroundServices;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Misc.Logger;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenDMSBackend.Tests.Testcases.Services
{
    [TestClass]
    public class ManagementServiceTests
    {
        private ManagementService CreateManagementService(ISet<ImportDefinition> importDefinitions, out Mock<IBusinessLogicService> businessLogicServiceMock)
        {
            return this.CreateManagementService(importDefinitions, out businessLogicServiceMock, out _);
        }

        private ManagementService CreateManagementService(ISet<ImportDefinition> importDefinitions, out Mock<IBusinessLogicService> businessLogicServiceMock, out IPersistence persistence)
        {
            ApplicationConstants<CodeUnitSpecificConstants> constants = new ApplicationConstants<CodeUnitSpecificConstants>(GeneralConstants.CodeUnitName, GeneralConstants.CodeUnitVersion, Version3.Parse(GeneralConstants.CodeUnitVersion), TestRun.Instance, OpenDMSBackend.Core.Misc.Utilities.GetEnvironmentTargetType(), new CodeUnitSpecificConstants());
            constants.BaseFolder = APIServer<CodeUnitSpecificConstants, PersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>, CommandlineParameter>.GetDefaultBaseFolder(constants, true);
            IManagementServiceLog logger = new ManagementServiceLog(new GRYLogConfiguration(true), constants.GetLogFolder());
            IAuditLog auditLog = new AuditLog(new GRYLogConfiguration(true), constants.GetLogFolder());
            IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration = new PersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>();
            configuration.ApplicationSpecificConfiguration = new CodeUnitSpecificConfiguration();
            configuration.ApplicationSpecificConfiguration.ImportDefinitions = importDefinitions;
            ITimeService timeService = new TimeService();
            (TransientPersistence, ISet<IDisposable>) persistenceD = OpenDMSBackend.Tests.TestUtilities.Utilities.GetTransientPersistence(timeService);
            persistence = persistenceD.Item1;
            IGeneralResourceLoader generalResourceLoader = new OpenDMSBackend.Core.Services.GeneralResourceLoader();
            businessLogicServiceMock = new Mock<IBusinessLogicService>(MockBehavior.Loose);
            return new ManagementService(logger, auditLog, configuration, persistenceD.Item1, constants, generalResourceLoader, businessLogicServiceMock.Object);
        }

        [TestMethod(DisplayName = nameof(ImportNewDocumentsFromFileSystemTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public void ImportNewDocumentsFromFileSystemTest()
        {
            //arrange
            string sourceFolder = Path.Combine(Path.GetTempPath(), "OpenDMSImportTest_" + Guid.NewGuid());
            Directory.CreateDirectory(sourceFolder);
            try
            {
                string fileName = "invoice.txt";
                byte[] fileContent = new byte[] { 1, 2, 3, 4 };
                string filePath = Path.Combine(sourceFolder, fileName);
                File.WriteAllBytes(filePath, fileContent);
                ImportDefinition importDefinition = new ImportDefinition() { Name = "test-import", IsActive = true, SourceLocationURL = sourceFolder, TargetFolderId = "target-folder-id" };
                ManagementService managementService = this.CreateManagementService(new HashSet<ImportDefinition> { importDefinition }, out Mock<IBusinessLogicService> businessLogicServiceMock);

                //act
                managementService.ImportNewDocuments();

                //assert
                businessLogicServiceMock.Verify(service => service.AddDocument(null, null, "target-folder-id", fileName, It.Is<byte[]>(content => content.SequenceEqual(fileContent)), It.IsAny<string>(), It.IsAny<ISet<string>>()), Times.Once);
                Assert.IsFalse(File.Exists(filePath), "The source-file must be removed after a successful import.");
            }
            finally
            {
                if (Directory.Exists(sourceFolder))
                {
                    Directory.Delete(sourceFolder, true);
                }
            }
        }

        [TestMethod(DisplayName = nameof(InactiveImportDefinitionIsSkippedTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public void InactiveImportDefinitionIsSkippedTest()
        {
            //arrange
            string sourceFolder = Path.Combine(Path.GetTempPath(), "OpenDMSImportTest_" + Guid.NewGuid());
            Directory.CreateDirectory(sourceFolder);
            try
            {
                string filePath = Path.Combine(sourceFolder, "invoice.txt");
                File.WriteAllBytes(filePath, new byte[] { 1, 2, 3, 4 });
                ImportDefinition importDefinition = new ImportDefinition() { Name = "inactive-import", IsActive = false, SourceLocationURL = sourceFolder, TargetFolderId = "target-folder-id" };
                ManagementService managementService = this.CreateManagementService(new HashSet<ImportDefinition> { importDefinition }, out Mock<IBusinessLogicService> businessLogicServiceMock);

                //act
                managementService.ImportNewDocuments();

                //assert
                businessLogicServiceMock.Verify(service => service.AddDocument(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<ISet<string>>()), Times.Never);
                Assert.IsTrue(File.Exists(filePath), "An inactive import-definition must not touch the source-files.");
            }
            finally
            {
                if (Directory.Exists(sourceFolder))
                {
                    Directory.Delete(sourceFolder, true);
                }
            }
        }

        [TestMethod(DisplayName = nameof(AdaptScriptModifiesScalarMetadataTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public void AdaptScriptModifiesScalarMetadataTest()
        {
            //arrange
            if (!IsTypeScriptCompilerAvailable())
            {
                Assert.Inconclusive("The TypeScript-compiler (tsc) is not available in this environment; the adapt-script can not be executed.");
                return;
            }
            ImportDefinition importDefinition = new ImportDefinition()
            {
                Name = "adapt-import",
                IsActive = true,
                SourceLocationURL = "unused",
                TargetFolderId = "target-folder-id",
                AdaptDocumentScriptBody = "document.Title = \"adapted-title\";\ndocument.GroupOfBusinessOwner = \"OU42\";\ndocument.DeleteIsNotAllowedBefore = new Date(\"2030-01-01T00:00:00Z\");\ndocument.MustBeHardDeletedAfter = null;"
            };
            ManagementService managementService = this.CreateManagementService(new HashSet<ImportDefinition> { importDefinition }, out _, out _);
            Document document = this.CreateTestDocument();

            //act
            managementService.RunAdaptScript(importDefinition, document);

            //assert
            Assert.AreEqual("adapted-title", document.Title.Value);
            Assert.AreEqual("OU42", document.GroupOfBusinessOwner);
            Assert.AreEqual(new DateTimeOffset(2030, 1, 1, 0, 0, 0, TimeSpan.Zero), document.DeleteIsNotAllowedBefore);
            Assert.IsNull(document.MustBeHardDeletedAfter);
        }

        [TestMethod(DisplayName = nameof(AdaptScriptAssignsTagByNameTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public void AdaptScriptAssignsTagByNameTest()
        {
            //arrange
            if (!IsTypeScriptCompilerAvailable())
            {
                Assert.Inconclusive("The TypeScript-compiler (tsc) is not available in this environment; the adapt-script can not be executed.");
                return;
            }
            ImportDefinition importDefinition = new ImportDefinition()
            {
                Name = "adapt-tag-import",
                IsActive = true,
                SourceLocationURL = "unused",
                TargetFolderId = "target-folder-id",
                AdaptDocumentScriptBody = "document.Tags.push(tools.getTagByName(\"Invoice\"));"
            };
            ManagementService managementService = this.CreateManagementService(new HashSet<ImportDefinition> { importDefinition }, out _, out IPersistence persistence);
            string tagId = Guid.NewGuid().ToString();
            persistence.CreateTag(new Tag(tagId, "Invoice", new ExtendedColor(255, 0, 0)));
            Document document = this.CreateTestDocument();

            //act
            ISet<string> resolvedTagIds = managementService.RunAdaptScript(importDefinition, document);

            //assert
            Assert.IsTrue(resolvedTagIds.Contains(tagId), "The tag resolved by name must be returned so that it can be assigned to the document.");
        }

        private Document CreateTestDocument()
        {
            return new Document(Guid.NewGuid().ToString(), OneLineString.From("original-title"), OneLineString.From("file.pdf"), OneLineString.From("file.pdf"), new DateTimeOffset(2026, 01, 02, 03, 04, 05, TimeSpan.Zero), 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3 }, string.Empty, new byte[] { 1 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new HashSet<string>(), null);
        }

        private static bool IsTypeScriptCompilerAvailable()
        {
            try
            {
                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                ProcessStartInfo processStartInfo = new ProcessStartInfo()
                {
                    FileName = isWindows ? "cmd.exe" : "/bin/bash",
                    Arguments = isWindows ? "/c tsc --version" : "-c \"tsc --version\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using Process? process = Process.Start(processStartInfo);
                if (process == null)
                {
                    return false;
                }
                process.WaitForExit(30000);
                return process.HasExited && process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
