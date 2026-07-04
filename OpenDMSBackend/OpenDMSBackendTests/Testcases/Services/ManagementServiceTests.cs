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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackend.Core.BackgroundServices;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Misc.Logger;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenDMSBackend.Tests.Testcases.Services
{
    [TestClass]
    public class ManagementServiceTests
    {
        private ManagementService CreateManagementService(ISet<ImportDefinition> importDefinitions, out Mock<IBusinessLogicService> businessLogicServiceMock)
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
    }
}
