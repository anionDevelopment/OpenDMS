using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Misc;

namespace OpenDMSBackend.Core.Services
{
    public class SQLProviderMariaDB : SQLProvider
    {
        public SQLProviderMariaDB(IGRYLog log) : base("MariaDB", log) { }
    }
}
