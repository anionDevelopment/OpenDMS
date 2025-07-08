using GRYLibrary.Core.APIServer.Utilities;
using Microsoft.EntityFrameworkCore;
using OpenDMSBackend.Core.Database;
using System.Data.Common;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFrameworkForPostgreSQL : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFrameworkForPostgreSQL() : base("opendmsbackend_database", "Host=localhost; Port=5432; Username=user; Database=OpenDMSDatabase; Password=pa55w0rd; Database=OpenDMSDatabase;", Utilities.GetTestPostgreSQLDatabaseFolder())
        {
        }



        public override void ConfigureDb<TDbContext>(DbContextOptionsBuilder<TDbContext> optionsBuilder)
        {
            optionsBuilder.UseNpgsql(this.ConnectionString);
        }

        public override DbConnection CreateConnection(string connectionString)
        {
            return new Npgsql.NpgsqlConnection(connectionString);
        }

        public override string GetDatabaseName()
        {
            return "PostgreSQL";
        }
    }
}
