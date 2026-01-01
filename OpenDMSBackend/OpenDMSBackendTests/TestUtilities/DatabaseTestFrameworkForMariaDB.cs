using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GRYLogger;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFrameworkForMariaDB : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFrameworkForMariaDB(IGRYLog log) : base("opendms_database_mariadb", new DatabasePersistenceConfiguration() { DatabaseType = "MariaDB", DatabaseConnectionString = Utilities.GetTestMariaDBConnectionString() }, Utilities.GetTestMariaDBDatabaseFolder(), OpenDMSBackend.Tests.TestUtilities.Constants.GeneralConstants.RepositoryFolder, "LocaltestserviceMariadbStart", "LocaltestserviceMariadbStop", OpenDMSBackend.Tests.TestUtilities.Utilities.GetResetDatabaseScript("MariaDB"), log)
        {
        }

        public override string GetDatabaseTypeName()
        {
            return "MariaDB";
        }
    }
}
