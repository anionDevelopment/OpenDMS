using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Misc;

namespace OpenDMSBackend.Core.Services
{
    public class SQLProviderPostgreSQL : SQLProvider
    {
        public SQLProviderPostgreSQL(IGRYLog log) : base("PostgreSQL", log) { }
    }
}
