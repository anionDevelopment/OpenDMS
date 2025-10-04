using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFrameworkForPostgreSQL : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFrameworkForPostgreSQL( ) : base("opendmsbackend_database", "Host=localhost; Port=5432; Username=user; Password=pa55w0rd; Database=OpenDMSDatabase;", Utilities.GetTestPostgreSQLDatabaseFolder(), OpenDMSBackend.Tests.TestUtilities.Constants.GeneralConstants.RepositoryFolder, "LocaltestservicePostgresqlStart", "LocaltestservicePostgresqlStop", OpenDMSBackend.Tests.TestUtilities.Utilities.GetResetDatabaseScript("PostgreSQL"))
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
