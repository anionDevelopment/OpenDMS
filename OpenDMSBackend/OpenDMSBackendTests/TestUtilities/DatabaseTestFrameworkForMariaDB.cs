using GRYLibrary.Core.APIServer.Utilities;
using MySqlConnector;
using System.Data.Common;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFrameworkForMariaDB : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFrameworkForMariaDB() : base("opendmsbackend_mariadbdatabase", "Host=localhost;Port=3306;Username=user;Password=pa55w0rd;Database=OpenDMSDatabase", Utilities.GetTestMariaDBDatabaseFolder() )
        {
        }

        public override DbConnection CreateConnection(string connectionString)
        {
            return new MySqlConnection(connectionString);
        }

        public override string GetDatabaseName()
        {
            return "MariaDB";
        }
    }
}
