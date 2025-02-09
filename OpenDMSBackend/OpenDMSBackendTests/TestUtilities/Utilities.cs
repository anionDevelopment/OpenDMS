using OpenDMSBackend.Tests.Constants;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public static class Utilities
    {
        public static string GetTestDatabaseFolder()
        {
            return GUtilities.ResolveToFullPath(@$"{GeneralConstants.RepositoryFolder}\Other\Resources\LocalTestServices\Database");
        }

        public static string GetTestDatabaseCreationScriptArtifactFolder()
        {
            return GUtilities.ResolveToFullPath(@$"{GeneralConstants.CodeUnitFolder}\Other\Artifacts\DatabaseCreationScript");
        }
    }
}
