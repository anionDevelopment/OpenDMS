namespace OpenDMSBackend.Core.Configuration
{
    public class DatabasePersistenceConfiguration : IDatabasePersistenceConfiguration
    {
        public string DatabaseConnectionString { get; set; }
        public string DatabaseType { get; set; }
    }
}
