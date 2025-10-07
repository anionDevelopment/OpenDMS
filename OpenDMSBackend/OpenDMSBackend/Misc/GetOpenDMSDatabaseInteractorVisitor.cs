using GRYLibrary.Core.APIServer.Services.Database;
using OpenDMSBackend.Core.Services;
using System;

namespace OpenDMSBackend.Core.Misc
{
    public class GetOpenDMSDatabaseInteractorVisitor : IGenericDatabaseInteractorVisitor<IOpenDMSDatabaseInteractor>
    {

        public IOpenDMSDatabaseInteractor Handle(MariaDBDatabaseInteractor mariaDBDatabaseInteractor)
        {
            return new DatabaseInteractorMariaDB(mariaDBDatabaseInteractor);
        }

        public IOpenDMSDatabaseInteractor Handle(OracleDatabaseInteractor oracleDatabaseInteractor)
        {
            throw new NotSupportedException();
        }

        public IOpenDMSDatabaseInteractor Handle(SQLServerDatabaseInteractor sQLServerDatabaseInteractor)
        {
            throw new NotSupportedException();
        }

        public IOpenDMSDatabaseInteractor Handle(PostgreSQLDatabaseInteractor postgreSQLDatabaseInteractor)
        {
            return new DatabaseInteractorPostgreSQL(postgreSQLDatabaseInteractor);
        }
    }
}
