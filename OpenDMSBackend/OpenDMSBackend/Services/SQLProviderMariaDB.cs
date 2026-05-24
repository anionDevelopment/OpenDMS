using OpenDMSBackend.Core.Misc;

namespace OpenDMSBackend.Core.Services
{
    public class SQLProviderMariaDB : SQLProvider
    {
        /// <summary>Initializes a new <see cref="SQLProviderMariaDB"/> pointing to the MariaDB statement resources.</summary>
        public SQLProviderMariaDB() : base("MariaDB") { }
    }
}
