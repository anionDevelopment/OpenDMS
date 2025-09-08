using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities.Constants;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using IdGenerator = OpenDMSBackend.Core.Services.IdGenerator;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public static class Utilities
    {
        internal static readonly object IntegrationTestLock=new object();
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
        public static string GetTestPostgreSQLDatabaseFolder()
        {
            return GUtilities.ResolveToFullPath(@$"{GeneralConstants.RepositoryFolder}\Other\Resources\LocalTestServices\PostgreSQLDatabase");
        }

        public static string GetTestDatabaseCreationScriptArtifactFolder(string databaseName)
        {
            return GUtilities.ResolveToFullPath(@$"{GeneralConstants.CodeUnitFolder}\Other\Artifacts\${databaseName}DatabaseCreationScript");
        }

        internal static string GetOCRDataFolder()
        {
            var result= @$"{OpenDMSBackend.Tests.TestUtilities.Constants.GeneralConstants.CodeUnitFolder}\Other\Resources\OCRData";
            return result;
        }
    }
}
