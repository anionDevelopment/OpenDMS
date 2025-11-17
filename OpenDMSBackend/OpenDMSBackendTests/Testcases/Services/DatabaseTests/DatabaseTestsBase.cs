using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.Migration;
using GRYLibrary.Core.Misc.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Misc;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDMSBackend.Tests.Testcases.Services.DatabaseTests
{
    public abstract class DatabaseTestsBase
    {
        protected abstract DatabaseTestFrameworkTemplate GetDatabaseTestFrameworkImplementation();
        protected DatabaseTestFrameworkTemplate GetDatabaseTestFramework()
        {
            DatabaseTestFrameworkTemplate result = this.GetDatabaseTestFrameworkImplementation();
            this.prepareDatabase(result);
            return result;
        }

        private void prepareDatabase(DatabaseTestFrameworkTemplate databaseTestFramework)
        {
            databaseTestFramework.ResetDatabase();
            IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
            IOpenDMSDatabaseInteractor openDMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenDMSDatabaseInteractorVisitor());

            List<string> tables1 = databaseInteractor.GetAllTableNames().ToList();
            Assert.IsEmpty(tables1);

            IList<MigrationInstance> migrations = openDMSDatabaseInteractor.GetAllMigrations();
            GRYMigrator migrator = new GRYMigrator(new TimeService(), migrations.ToList(), databaseInteractor);

            migrator.InitializeDatabaseAndMigrateIfRequired();
        }
        public abstract void Migration000001Test();
        public void Migration000001()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                using (DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework())
                {
                    //arrange
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
                    //arrange
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
                    //TODO add more migration-specific assertions
                }
            }
        }
        public abstract void LoadDocumentTest();
        public void LoadDocument()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                using (DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework())
                {
                    //arrange
                    IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
                    IOpenDMSDatabaseInteractor openDMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenDMSDatabaseInteractorVisitor());
                    ITimeService timeService = new TimeService();
                    IGRYLog log = GRYLog.Create();
                    DatabasePersistence databasePersistence = new DatabasePersistence(openDMSDatabaseInteractor, timeService, log);
                    OpenDMSBackend.Core.Model.BusinessTypes.Document expectedDocument = new OpenDMSBackend.Core.Model.BusinessTypes.Document("id", OneLineString.From("title"), OneLineString.From("filename"), OneLineString.From("originalfilename"),new System.DateTimeOffset(2025,11,17,17,54,38,TimeSpan.FromHours(2)),default,2,new HashSet<Tag>(),OneLineString.From("mimetype"),new byte[] { 2,3,4},"ocrcontent", new byte[] { 5,6,7},false, new System.DateTimeOffset(2026, 11, 17, 17, 54, 38, TimeSpan.FromHours(2)), new System.DateTimeOffset(2027, 11, 17, 17, 54, 38, TimeSpan.FromHours(2)),"ownergroup",new GRYLibrary.Core.Misc.Version3(2,3,4),new HashSet<string>() { "eng","deu","fra"},"creator-user-id");
                    databasePersistence.CreateDocument(expectedDocument);

                    //act
                    Core.Model.BusinessTypes.Document actualDocument = databasePersistence.GetDocument(expectedDocument.Id);

                    //assert
                    Assert.AreEqual(expectedDocument, actualDocument);
                }
            }
        }
    }
}
