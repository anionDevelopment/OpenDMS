using GRYLibrary.Core.APIServer.Utilities;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFramework : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFramework() : base("opendms_database", "Server=localhost; Port=3306; Database=OpenDMSDatabase; Uid=user; Pwd=pa55w0rd;", Utilities.GetTestDatabaseFolder())
        {
        }
    }
}
