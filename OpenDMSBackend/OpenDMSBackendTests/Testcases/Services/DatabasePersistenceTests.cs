using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackendUtilities = OpenDMSBackend.Core.Miscellaneous.Utilities;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Database;
using OpenDMSBackend.Tests.TestUtilities;
using System;
using System.IO;
using System.Text;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using OpenDMSBackend.Core.Configuration;
using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Services;
using GRYLibrary.Core.Misc.Strings;
using MySqlConnector;
using System.Collections.Generic;
using OpenDMSBackend.Core.Model.BusinessTypes;
using GRYLibrary.Core.APIServer.Services.OtherServices;

namespace OpenDMSBackend.Tests.Testcases.Services
{
    [TestClass]
    public class DatabasePersistenceTests
    {
        [TestMethod(nameof(DatabasePersistenceCreateDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public void DatabasePersistenceCreateDocumentTest()
        {
            //arrange
            using (DatabaseTestFramework databaseTestFramework = new DatabaseTestFramework())
            {
                IDatabaseManager databaseManager = new DatabaseManager();
                ITimeService timeService = new TimeService();
                GRYMigrator.DoAllMigrations(databaseTestFramework.MySqlConnection, databaseManager, timeService);
                DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
                optionsBuilder.UseMySql(databaseTestFramework.ConnectionString, ServerVersion.AutoDetect(databaseTestFramework.ConnectionString));
                IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> persistedAPIServerConfiguration = new PersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>();
                persistedAPIServerConfiguration.ApplicationSpecificConfiguration = new CodeUnitSpecificConfiguration();
                persistedAPIServerConfiguration.ApplicationSpecificConfiguration.RegistrationIsEnabled = true;
                Mock<IExampleDataCreator> exampleDataCreatorMock = new Mock<IExampleDataCreator>(MockBehavior.Strict);
                Mock<IIdGenerator<ulong>> idGeneratorMock = new Mock<IIdGenerator<ulong>>(MockBehavior.Strict);
                exampleDataCreatorMock.Setup(mock => mock.AddExampleData());
                IGRYLog logger = GeneralLogger.CreateUsingConsole();
                Mock<IApplicationConstants<CodeUnitSpecificConstants>> constantsMock = new Mock<IApplicationConstants<CodeUnitSpecificConstants>>(MockBehavior.Strict);
                constantsMock.SetupGet(m => m.Environment).Returns(OpenDMSBackendUtilities.GetEnvironmentTargetType());
                ISQLProvider sqlProvider = new SQLProvider();

                DatabasePersistence databasePersistence = new DatabasePersistence(optionsBuilder.Options, logger, timeService, databaseManager, logger, sqlProvider);

                Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), timeService.GetCurrentTimeAsGRYDateTime(), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 });

                //act
                databasePersistence.CreateDocument(testDocument);

                // assert
                string reloadedDocumentOriginalFilename = databasePersistence.RunTransaction((cmd) =>
                {
                    cmd.CommandText = $"select OriginalFilename from Documents where Id='{testDocument.Id:N}'";
                    using MySqlDataReader reader = cmd.ExecuteReader();
                    Assert.IsTrue(reader.HasRows);
                    reader.Read();
                    return reader.GetString(0);
                })[0];
                Assert.AreEqual(testDocument.OriginalFilename.Value, reloadedDocumentOriginalFilename);
            }
        }
        [TestMethod(nameof(GetAmountOfDocumentsTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public void GetAmountOfDocumentsTest()
        {
            //arrange
            using (DatabaseTestFramework databaseTestFramework = new DatabaseTestFramework())
            {
                IDatabaseManager databaseManager = new DatabaseManager();
               ITimeService timeService = new TimeService();
                GRYMigrator.DoAllMigrations(databaseTestFramework.MySqlConnection, databaseManager, timeService);
                DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
                optionsBuilder.UseMySql(databaseTestFramework.ConnectionString, ServerVersion.AutoDetect(databaseTestFramework.ConnectionString));
                 IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> persistedAPIServerConfiguration = new PersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>();
                persistedAPIServerConfiguration.ApplicationSpecificConfiguration = new CodeUnitSpecificConfiguration();
                persistedAPIServerConfiguration.ApplicationSpecificConfiguration.RegistrationIsEnabled = true;
                Mock<IIdGenerator<ulong>> idGeneratorMock = new Mock<IIdGenerator<ulong>>(MockBehavior.Strict);
                Mock<IExampleDataCreator> exampleDataCreatorMock = new Mock<IExampleDataCreator>(MockBehavior.Strict);
                exampleDataCreatorMock.Setup(mock => mock.AddExampleData());
                IGRYLog logger = GeneralLogger.CreateUsingConsole();
                Mock<IApplicationConstants<CodeUnitSpecificConstants>> constantsMock = new Mock<IApplicationConstants<CodeUnitSpecificConstants>>(MockBehavior.Strict);
                constantsMock.SetupGet(m => m.Environment).Returns(OpenDMSBackendUtilities.GetEnvironmentTargetType());
                ISQLProvider sqlProvider = new SQLProvider();

                DatabasePersistence databasePersistence = new DatabasePersistence(optionsBuilder.Options, logger, timeService, databaseManager, logger, sqlProvider);

                Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), timeService.GetCurrentTimeAsGRYDateTime(), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 });

                uint expectedAmount1 = 0;
                uint expectedAmount2 = 1;

                //act
                uint actualAmount1 = databasePersistence.GetAmountOfDocuments();
                databasePersistence.CreateDocument(testDocument);
                uint actualAmount2 = databasePersistence.GetAmountOfDocuments();

                // assert

                Assert.AreEqual(expectedAmount1, actualAmount1);
                Assert.AreEqual(expectedAmount2, actualAmount2);
            }
        }

        [TestMethod(nameof(GenerateDatabaseGenerationScript))]
        [TestProperty(nameof(TestKind), nameof(TestKind.GenerationTest))]
        public void GenerateDatabaseGenerationScript()
        {
            using DatabaseTestFramework databaseTestFramework = new DatabaseTestFramework();
            IDatabaseManager databaseManager = new DatabaseManager();
            ITimeService timeService = new TimeService();
            GRYMigrator.DoAllMigrations(databaseTestFramework.MySqlConnection, databaseManager, timeService);
            DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseMySql(databaseTestFramework.ConnectionString, ServerVersion.AutoDetect(databaseTestFramework.ConnectionString));
            DatabaseContext context = new DatabaseContext(optionsBuilder.Options, GeneralLogger.CreateUsingConsole(), new TimeService(), databaseManager);
            string sqlSource = context.Database.GenerateCreateScript();
            string targetFolder = TestUtilities.Utilities.GetTestDatabaseCreationScriptArtifactFolder();
            GUtilities.EnsureDirectoryDoesNotExist(targetFolder);
            GUtilities.EnsureDirectoryExists(targetFolder);
            string targetFile = Path.Join(targetFolder, "CreateDatabase.sql");
            File.WriteAllText(targetFile, sqlSource, new UTF8Encoding(false));
        }

    }
}
