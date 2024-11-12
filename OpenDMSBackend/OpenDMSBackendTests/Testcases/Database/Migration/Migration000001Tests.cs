using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Services.TS;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Database;
using OpenDMSBackend.Tests.TestUtilities;
using System.Collections.Generic;
using System.Linq;

namespace OpenDMSBackend.Tests.Testcases.Database.Migration
{
    [TestClass]
    public class Migration000001Tests
    {
        [TestMethod(nameof(Migration000001Test))]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public void Migration000001Test()
        {
            //arrange
            using DatabaseTestFramework databaseTestFramework = new DatabaseTestFramework();
            IDatabaseManager databaseManager = new DatabaseManager();
            IList<MigrationInstance> migrations = databaseManager.GetAllMigrations();
            Assert.IsFalse(databaseManager.GetGenericDatabaseInteractor().GetAllTableNames(databaseTestFramework.MySqlConnection).Any());
            GRYMigrator migrator = new GRYMigrator(GeneralLogger.CreateUsingConsole(), new TimeService(), databaseTestFramework.MySqlConnection, migrations.Take(1).ToList(), databaseManager.GetGenericDatabaseInteractor());

            //act
            migrator.InitializeDatabaseAndMigrateIfRequired();

            //assert
            Assert.IsTrue(databaseManager.GetGenericDatabaseInteractor().GetAllTableNames(databaseTestFramework.MySqlConnection).Any());
            Assert.AreEqual(1, migrator.GetExecutedMigrations().Count);
            Assert.AreEqual("Migration000001", migrator.GetExecutedMigrations().First().MigrationName);
            //TODO add the migration-specific assertions
        }
    }
}
