using GRYLibrary.Core.APIServer.Utilities;
using System.Data.Common;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFrameworkForPostgreSQL : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFrameworkForPostgreSQL() : base("opendmsbackend_postgresqldatabase", "Host=localhost;Port=5432;Username=user;Password=pa55w0rd;Database=OpenDMSDatabase", Utilities.GetTestPostgreSQLDatabaseFolder())
        {
        }

        public override DbConnection CreateConnection(string connectionString)
        {
          return new Npgsql.NpgsqlConnection(connectionString) { 
          
          };
        }

        public override string GetDatabaseName()
        {
            return "PostgreSQL";
        }
    }
}
