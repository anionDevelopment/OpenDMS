using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Miscellaneous;

namespace OpenDMSBackend.Core.Services
{
    public class SQLProviderPostgreSQL : SQLProvider
    {
        public SQLProviderPostgreSQL(IGRYLog log) : base("PostgreSQL", log) { }
    }
}
