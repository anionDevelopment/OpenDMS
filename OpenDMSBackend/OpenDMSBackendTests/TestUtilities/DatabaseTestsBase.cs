using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Database;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public abstract class DatabaseTestsBase
    {
        protected abstract DatabaseTestFrameworkTemplate GetDatabaseTestFramework();


        public abstract void Migration000001Test();
        public void Migration000001()
        {
            //arrange
            using DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework();
            IDatabaseManager databaseManager = new DatabaseManagerPostgreSQL();
            IList<MigrationInstance> migrations = databaseManager.GetAllMigrations();
            Assert.IsFalse(databaseManager.GetGenericDatabaseInteractor().GetAllTableNames(databaseTestFramework.Connection).Any());
            GRYMigrator migrator = new GRYMigrator(GeneralLogger.CreateUsingConsole(), new TimeService(), databaseTestFramework.Connection, migrations.Take(1).ToList(), databaseManager.GetGenericDatabaseInteractor());

            //act
            migrator.InitializeDatabaseAndMigrateIfRequired();

            //assert
            Assert.IsTrue(databaseManager.GetGenericDatabaseInteractor().GetAllTableNames(databaseTestFramework.Connection).Any());
            Assert.AreEqual(1, migrator.GetExecutedMigrations().Count);
            Assert.AreEqual("Migration000001", migrator.GetExecutedMigrations().First().MigrationName);
            //TODO add the migration-specific assertions
        }

        public abstract void GenerateDatabaseGenerationScriptTest();
        public void GenerateDatabaseGenerationScript()
        {
            using DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework();
            IDatabaseManager databaseManager = new DatabaseManagerPostgreSQL();
            ITimeService timeService = new TimeService();
            GRYMigrator.DoAllMigrations(databaseTestFramework.Connection, databaseManager, timeService);
            DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseMySql(databaseTestFramework.ConnectionString, ServerVersion.AutoDetect(databaseTestFramework.ConnectionString));
            DatabaseContext context = new DatabaseContext(optionsBuilder.Options, GeneralLogger.CreateUsingConsole(), new TimeService(), databaseManager);
            string sqlSource = context.Database.GenerateCreateScript();
            string targetFolder = TestUtilities.Utilities.GetTestDatabaseCreationScriptArtifactFolder(databaseTestFramework.GetDatabaseName());
            GUtilities.EnsureDirectoryDoesNotExist(targetFolder);
            GUtilities.EnsureDirectoryExists(targetFolder);
            string targetFile = Path.Join(targetFolder, "CreateDatabase.sql");
            File.WriteAllText(targetFile, sqlSource, new UTF8Encoding(false));
        }

    }
}
