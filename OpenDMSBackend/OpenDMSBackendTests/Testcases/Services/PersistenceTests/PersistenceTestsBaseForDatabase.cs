using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Misc;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDMSBackend.Tests.Testcases.Services.PersistenceTests
{
    public abstract class PersistenceTestsBaseForDatabase : PersistenceTestsBase
    {
        protected abstract DatabaseTestFrameworkTemplate GetDatabaseTestFramework();
        internal override PersistenceDisposable GetPersistence(ITimeService timeService)
        {
            DatabaseTestFrameworkTemplate databaseTestFramework = this.GetDatabaseTestFramework();
            IGRYLog logger = GeneralLogger.CreateUsingConsole();
            IPersistence result = new DatabasePersistence(databaseTestFramework.GenericDatabaseInteractor().Accept(new GetOpenDMSDatabaseInteractorVisitor()), timeService, logger);

            databaseTestFramework.ResetDatabase();
            IGenericDatabaseInteractor databaseInteractor = databaseTestFramework.GenericDatabaseInteractor();
            IOpenDMSDatabaseInteractor openDMSDatabaseInteractor = databaseInteractor.Accept(new GetOpenDMSDatabaseInteractorVisitor());

            List<string> tables1 = databaseInteractor.GetAllTableNames().ToList();
            Assert.IsEmpty(tables1);

            IList<MigrationInstance> migrations = openDMSDatabaseInteractor.GetAllMigrations();
            GRYMigrator migrator = new GRYMigrator(new TimeService(), migrations.ToList(), databaseInteractor);

            migrator.InitializeDatabaseAndMigrateIfRequired();

            return new PersistenceDisposable(result, new HashSet<IDisposable>() { databaseTestFramework, databaseInteractor });
        }
    }
}
