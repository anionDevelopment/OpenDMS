using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GRYLogger;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFrameworkForPostgreSQL : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFrameworkForPostgreSQL(IGRYLog log) : base("opendms_database_postgresql", new DatabasePersistenceConfiguration() { DatabaseType = "PostgreSQL", DatabaseConnectionString = Utilities.GetTestPostgreSQLConnectionString() }, Utilities.GetTestPostgreSQLDatabaseFolder(), OpenDMSBackend.Tests.TestUtilities.Constants.GeneralConstants.RepositoryFolder, "LocaltestservicePostgresqlStart", "LocaltestservicePostgresqlStop", OpenDMSBackend.Tests.TestUtilities.Utilities.GetResetDatabaseScript("PostgreSQL"), log)
        {
        }

        public override string GetDatabaseTypeName()
        {
            return "PostgreSQL";
        }
    }
}
