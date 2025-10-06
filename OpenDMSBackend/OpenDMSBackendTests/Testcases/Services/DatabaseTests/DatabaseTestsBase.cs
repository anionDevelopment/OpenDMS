using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace OpenDMSBackend.Tests.Testcases.Services.DatabaseTests
{
    public abstract class DatabaseTestsBase
    {
        protected abstract DatabaseTestFrameworkTemplate GetDatabaseTestFramework();
        public IOpenDMSDatabaseInteractor DatabaseManager { get; private set; }
        public abstract void Migration000001Test();
        protected static readonly object DatabaseTestsLockObject = new object();
        public DatabaseTestsBase(IOpenDMSDatabaseInteractor genericDatabaseInteractor)
        {
            DatabaseManager = genericDatabaseInteractor;
        }
        public void Migration000001IsWorking()
        {
            //arrange
            using (DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework())
            {
                IOpenDMSDatabaseInteractor databaseManager = DatabaseManager;
                IList<MigrationInstance> migrations = databaseManager.GetAllMigrations();
               var migrator = new GRYMigrator(new TimeService(), migrations.Take(1).ToList(), databaseManager);
                List<string> tables1 = databaseManager.GetAllTableNames().ToList();
                Assert.IsEmpty(tables1);

                //act
                migrator.InitializeDatabaseAndMigrateIfRequired();

                //assert
                List<string> tables2 = databaseManager.GetAllTableNames().ToList();
                Assert.IsEmpty(tables2);
                Assert.AreEqual(1, migrator.GetExecutedMigrations().Count);
                Assert.AreEqual("Migration000001", migrator.GetExecutedMigrations().First().MigrationName);
                //TODO add migration-specific assertions
            }
        }

        public abstract void GenerateDatabaseGenerationScriptTest();
        public void GenerateDatabaseGenerationScript()
        {
            using (DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework())
            {
                IDatabaseManager databaseManager = this.GetDatabaseManager();
                ITimeService timeService = new TimeService();
                GRYMigrator.DoAllMigrations(databaseTestFramework.Connection, databaseManager, timeService);
                DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
                databaseTestFramework.ConfigureDb(optionsBuilder);
                DatabaseContext context = new DatabaseContext(optionsBuilder.Options, GeneralLogger.CreateUsingConsole(), new TimeService(), databaseManager, databaseTestFramework.Connection);
                string sqlSource = context.Database.GenerateCreateScript();
                string targetFolder = Utilities.GetTestDatabaseCreationScriptArtifactFolder(databaseTestFramework.GetDatabaseName());
                GUtilities.EnsureDirectoryDoesNotExist(targetFolder);
                GUtilities.EnsureDirectoryExists(targetFolder);
                string targetFile = Path.Join(targetFolder, "CreateDatabase.sql");
                File.WriteAllText(targetFile, sqlSource, new UTF8Encoding(false));
            }
        }
    }
}
