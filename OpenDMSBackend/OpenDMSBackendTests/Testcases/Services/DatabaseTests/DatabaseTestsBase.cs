using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Misc;
using OpenDMSBackend.Core.Services;
using System.Collections.Generic;
using System.Linq;

namespace OpenDMSBackend.Tests.Testcases.Services.DatabaseTests
{
    public abstract class DatabaseTestsBase
    {
        protected abstract DatabaseTestFrameworkTemplate GetDatabaseTestFramework();
        public abstract void Migration000001Test();
        public void Migration000001()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            { 
            //arrange
            using (DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework())
            {
                databaseTestFramework.ResetDatabase();
                IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
                IOpenDMSDatabaseInteractor openDMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenDMSDatabaseInteractorVisitor());

                List<string> tables1 = databaseInteractor.GetAllTableNames().ToList();
                Assert.IsEmpty(tables1);

                IList<MigrationInstance> migrations = openDMSDatabaseInteractor.GetAllMigrations();
                GRYMigrator migrator = new GRYMigrator(new TimeService(), migrations.Take(1).ToList(), databaseInteractor);

                //act
                migrator.InitializeDatabaseAndMigrateIfRequired();

                //assert
                Assert.AreEqual(1, migrator.GetExecutedMigrations().Count);
                Assert.AreEqual("Migration000001", migrator.GetExecutedMigrations().First().MigrationName);

                List<string> tables2 = databaseInteractor.GetAllTableNames().ToList();
                Assert.IsTrue(1 < tables2.Count);
                //TODO add more migration-specific assertions
            }
            }
        }


        public abstract void AllMigrationsAreWorkingTest();
        public void AllMigrationsAreWorking()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                using (DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework())
                {
                    databaseTestFramework.ResetDatabase();
                    IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
                    IOpenDMSDatabaseInteractor openDMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenDMSDatabaseInteractorVisitor());

                    List<string> tables1 = databaseInteractor.GetAllTableNames().ToList();
                    Assert.IsEmpty(tables1);

                    IList<MigrationInstance> migrations = openDMSDatabaseInteractor.GetAllMigrations();
                    GRYMigrator migrator = new GRYMigrator(new TimeService(), migrations.ToList(), databaseInteractor);

                    //act
                    migrator.InitializeDatabaseAndMigrateIfRequired();

                    //assert
                    Assert.AreEqual(migrations.Count, migrator.GetExecutedMigrations().Count);

                    List<string> tables2 = databaseInteractor.GetAllTableNames().ToList();
                    Assert.IsTrue(1 < tables2.Count);
                }
            }
        }
    }
}
