namespace OpenDMSBackend.Core.Configuration
{
    public interface IDatabasePersistenceConfiguration
    {
        public string DatabaseConnectionString { get; set; }
        public string DatabaseType { get; set; }
    }
}
