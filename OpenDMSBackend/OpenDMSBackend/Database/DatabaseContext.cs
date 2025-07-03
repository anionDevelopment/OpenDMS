using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace OpenDMSBackend.Core.Database
{
    public class DatabaseContext : DbContext
    {
        private readonly IGeneralLogger _Logger;
        private readonly ITimeService _TimeService;
        private readonly IDatabaseManager _DatabaseManager;
        internal DbConnection Connection { get; private set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options, IGeneralLogger logger, ITimeService timeService, IDatabaseManager databaseManager) : base(options)
        {
            this._Logger = logger;
            this._TimeService = timeService;
            this.Connection = this.Database.GetDbConnection();
            this._DatabaseManager = databaseManager;
            this.Initialize();
        }

        private void Initialize()
        {
            this.Connection.Open();
            GRYMigrator migrator = new GRYMigrator(this._Logger, this._TimeService, this.Connection, this._DatabaseManager.GetAllMigrations(), this._DatabaseManager.GetGenericDatabaseInteractor());
            migrator.InitializeDatabaseAndMigrateIfRequired();
        }
    }
}
