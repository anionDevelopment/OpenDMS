using GRYLibrary.Core.APIServer.Utilities;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFramework : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFramework() : base("OpenDMSBackendc_database", "Server=localhost; Port=3306; Database=OpenDMSBackendDatabase; Uid=user; Pwd=pa55w0rd;", Utilities.GetTestDatabaseFolder())
        {
        }
    }
}
