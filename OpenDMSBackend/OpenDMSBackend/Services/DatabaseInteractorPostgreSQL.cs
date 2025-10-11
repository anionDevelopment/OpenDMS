using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc.Migration;
using System.Collections.Generic;
using System.Reflection;

namespace OpenDMSBackend.Core.Services
{
    public class DatabaseInteractorPostgreSQL : IOpenDMSDatabaseInteractor
    {
        public IGRYLog Log
        {
            get
            {
                return this._DatabaseInteractor.Log;
            }
        }
        private readonly PostgreSQLDatabaseInteractor _DatabaseInteractor;
        private readonly IList<MigrationInstance> _Migrations = GRYMigrator.LoadMigrationsFromResources(Assembly.GetExecutingAssembly(), "OpenDMSBackend.Core.Resources.Database.PostgreSQL.Migrations.");
        public DatabaseInteractorPostgreSQL(IGenericDatabaseInteractor interactor)
        {
            this._DatabaseInteractor = (PostgreSQLDatabaseInteractor)interactor;
        }
        public IList<MigrationInstance> GetAllMigrations()
        {
            return this._Migrations;
        }

        public IGenericDatabaseInteractor GetGenericDatabaseInteractor()
        {
            return this._DatabaseInteractor;
        }

        public ISQLProvider GetSQLProvider()
        {
            return new SQLProviderPostgreSQL();
        }
    }
}
