using GRYLibrary.Core.APIServer.ConcreteEnvironments;

namespace OpenDMSBackend.Core.Miscellaneous
{
    internal static class Utilities
    {
     
        internal static GRYEnvironment GetEnvironmentTargetType()
        {
#if Development
            return Development.Instance;
#elif QualityCheck
            return QualityCheck.Instance;
#elif Productive
            return Productive.Instance;
#else
            throw new System.Collections.Generic.KeyNotFoundException("Unknown environmenttargettype.");
#endif
        }
    }
}
