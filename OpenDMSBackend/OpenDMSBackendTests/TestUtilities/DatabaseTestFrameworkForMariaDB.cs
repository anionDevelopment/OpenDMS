using GRYLibrary.Core.APIServer.Utilities;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFrameworkForMariaDB : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFrameworkForMariaDB() : base("opendmsbackend_postgresqldatabase", "postgresql://root:R00tpa55w0rd@opendms_database:5432/OpenDMSDatabase", Utilities.GetTestMariaDBDatabaseFolder())
        {
        }

        public override string GetDatabaseName()
        {
            return "MariaDB";
        }
    }
}
