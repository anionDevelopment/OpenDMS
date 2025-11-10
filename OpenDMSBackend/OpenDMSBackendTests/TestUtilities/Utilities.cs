using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities.Constants;
using System.IO;
using System.Text;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using IdGenerator = OpenDMSBackend.Core.Services.IdGenerator;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public static class Utilities
    {
        internal static readonly object LockForTests = new object();
        public static string GetResetDatabaseScript(string databaseName)
        {
            string file = Path.Combine(OpenDMSBackend.Tests.TestUtilities.Constants.GeneralConstants.CodeUnitFolder, "OpenDMSBackend", "Resources", "Database", databaseName, "Statements", "ResetDatabase.sql");
            string result = File.ReadAllText(file, new UTF8Encoding(false));
            return result;
        }
        public static TransientPersistence GetTransientPersistence()
        {
            ITimeService timeService = new TimeService();
            IIdGenerator<ulong> idGenerator = new IdGenerator();
            TransientAuthenticationServicePersistence<User> transientAuthenticationServicePersistence = new TransientAuthenticationServicePersistence(timeService);
            TransientPersistence persistence = new TransientPersistence(transientAuthenticationServicePersistence, idGenerator, timeService);
            return persistence;
        }

        public static string GetTestMariaDBDatabaseFolder()
        {
            return GUtilities.ResolveToFullPath(@$"{GeneralConstants.RepositoryFolder}\Other\Resources\LocalTestServices\MariaDBDatabase");
        }

        public static string GetTestMariaDBConnectionString()
        {
            return @$"Host=localhost;Port=3306;User ID=user;Password=pa55w0rd;Database=OpenDMSDatabase;";
        }
        public static string GetTestPostgreSQLDatabaseFolder()
        {
            return GUtilities.ResolveToFullPath(@$"{GeneralConstants.RepositoryFolder}\Other\Resources\LocalTestServices\PostgreSQLDatabase");
        }

        public static string GetTestPostgreSQLConnectionString()
        {
            return @$"Host=localhost; Port=5432; Username=user; Password=pa55w0rd; Database=OpenDMSDatabase;";
        }

        public static string GetTestDatabaseCreationScriptArtifactFolder(string databaseName)
        {
            return GUtilities.ResolveToFullPath(@$"{GeneralConstants.CodeUnitFolder}\Other\Artifacts\${databaseName}DatabaseCreationScript");
        }

        public static DatabaseTestFrameworkForMariaDB GetDatabaseTestFrameworkForMariaDB()
        {
            return new DatabaseTestFrameworkForMariaDB(GeneralLogger.CreateUsingConsole());
        }
        public static DatabaseTestFrameworkForPostgreSQL GetDatabaseTestFrameworkForPostgreSQL()
        {
            return new DatabaseTestFrameworkForPostgreSQL(GeneralLogger.CreateUsingConsole());
        }

        public static DatabaseInteractorMariaDB GetMariaDBTestDatabase()
        {
            return new DatabaseInteractorMariaDB(DBUtilities.ToGenericDatabaseInteractor(GetDatabaseConfigurationMariaDB(), GeneralLogger.CreateUsingConsole()));
        }

        public static DatabaseInteractorPostgreSQL GetPostgreSQLTestDatabase()
        {
            return new DatabaseInteractorPostgreSQL(DBUtilities.ToGenericDatabaseInteractor(GetDatabaseConfigurationPostgreSQL(), GeneralLogger.CreateUsingConsole()));
        }
        public static IDatabasePersistenceConfiguration GetDatabaseConfigurationMariaDB()
        {
            return new DatabasePersistenceConfiguration()
            {
                DatabaseType = "MariaDB",
                DatabaseConnectionString = OpenDMSBackend.Tests.TestUtilities.Utilities.GetTestMariaDBConnectionString()
            };
        }

        public static IDatabasePersistenceConfiguration GetDatabaseConfigurationPostgreSQL()
        {
            return new DatabasePersistenceConfiguration()
            {
                DatabaseType = "PostgreSQL",
                DatabaseConnectionString = OpenDMSBackend.Tests.TestUtilities.Utilities.GetTestPostgreSQLConnectionString()
            };
        }
    }
}
