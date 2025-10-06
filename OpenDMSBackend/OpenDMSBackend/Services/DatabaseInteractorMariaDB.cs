using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.Migration;
using System.Collections.Generic;
using System.Reflection;

namespace OpenDMSBackend.Core.Services
{
    public class DatabaseInteractorMariaDB : IOpenDMSDatabaseInteractor
    {
        public IGRYLog Log { get; private set; }
        private readonly MariaDBDatabaseInteractor _DatabaseInteractor;
        private readonly IList<MigrationInstance> _Migrations = GRYMigrator.LoadMigrationsFromResources(Assembly.GetExecutingAssembly(), "OpenDMSBackend.Core.Resources.Database.MariaDB.Migrations.");
        public DatabaseInteractorMariaDB(IDatabasePersistenceConfiguration configuration)
        {
            Log = GeneralLogger.CreateUsingConsole();
            _DatabaseInteractor = new MariaDBDatabaseInteractor(configuration, Log);
        }
        public IList<MigrationInstance> GetAllMigrations()
        {
            return this._Migrations;
        }

        public IGenericDatabaseInteractor GetGenericDatabaseInteractor()
        {
            return this._DatabaseInteractor;
        }
    }
}
