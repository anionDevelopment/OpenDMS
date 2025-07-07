namespace OpenDMSBackend.Core.Database.SupportedDatabases
{
    public interface ISupportedDatabase
    {
        public abstract void Accept(ISupportedDatabaseVisitor visitor);
        public abstract T Accept<T>(ISupportedDatabaseVisitor<T> visitor);
    }
    public interface ISupportedDatabaseVisitor
    {
        void Handle(MariaDB mariaDB);
        void Handle(PostgreSQL postgreSQL);
    }
    public interface ISupportedDatabaseVisitor<T>
    {
        T Handle(MariaDB mariaDB);
        T Handle(PostgreSQL postgreSQL);
    }
}
