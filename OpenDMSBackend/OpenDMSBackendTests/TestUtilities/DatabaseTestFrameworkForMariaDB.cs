using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data.Common;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFrameworkForMariaDB : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFrameworkForMariaDB(IGRYLog log) : base("opendmsbackend_database_mariadb", new DatabasePersistenceConfiguration() { DatabaseType = "MariaDB", DatabaseConnectionString = Utilities.GetTestMariaDBConnectionString() }, Utilities.GetTestMariaDBDatabaseFolder(), OpenDMSBackend.Tests.TestUtilities.Constants.GeneralConstants.RepositoryFolder, "LocaltestserviceMariaDBStart", "LocaltestserviceMariaDBStop", OpenDMSBackend.Tests.TestUtilities.Utilities.GetResetDatabaseScript("MariaDB"), log)
        {
        }

        public override string GetDatabaseName()
        {
            return "MariaDB";
        }
    }
}
