namespace OpenDMSBackend.Core.Database.SupportedDatabases
{
    public class MariaDB : ISupportedDatabase
    {
        public void Accept(ISupportedDatabaseVisitor visitor)
        {
            visitor.Handle(this);
        }

        public T Accept<T>(ISupportedDatabaseVisitor<T> visitor)
        {
           return visitor.Handle(this);
        }
    }
}
