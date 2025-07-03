using GRYLibrary.Core.APIServer.Utilities;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFrameworkForPostgreSQL : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFrameworkForPostgreSQL() : base("opendmsbackend_mariadbdatabase", "Server=localhost; Port=3306; Database=OpenDMSBackendDatabase; Uid=user; Pwd=pa55w0rd;", Utilities.GetTestPostgreSQLDatabaseFolder())
        {
        }

        public override string GetDatabaseName()
        {
            return "PostgreSQL";
        }
    }
}
