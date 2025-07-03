using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Misc.Migration;
using System.Collections.Generic;
using System.Reflection;

namespace OpenDMSBackend.Core.Database
{
    public class DatabaseManagerMariaDB : IDatabaseManager
    {
        private readonly MariaDBDatabaseInteractor _MariaDBDatabaseInteractor = new MariaDBDatabaseInteractor();
        private readonly IList<MigrationInstance> _Migrations = GRYMigrator.LoadMigrationsFromResources(Assembly.GetExecutingAssembly(), "OpenDMSBackend.Core.Resources.Database.Migrations.MariaDB");
        public IList<MigrationInstance> GetAllMigrations()
        {
            return this._Migrations;
        }

        public IGenericDatabaseInteractor GetGenericDatabaseInteractor()
        {
            return this._MariaDBDatabaseInteractor;
        }
    }
}
