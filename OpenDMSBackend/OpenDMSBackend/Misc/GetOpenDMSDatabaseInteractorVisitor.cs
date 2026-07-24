using GRYLibrary.Core.APIServer.Services.Database;
using OpenDMSBackend.Core.Services;
using System;

namespace OpenDMSBackend.Core.Misc
{
    public class GetOpenDMSDatabaseInteractorVisitor : IGenericDatabaseInteractorVisitor<IOpenDMSDatabaseInteractor>
    {

        /// <inheritdoc />
        public IOpenDMSDatabaseInteractor Handle(MariaDBDatabaseInteractor mariaDBDatabaseInteractor)
        {
            return new DatabaseInteractorMariaDB(mariaDBDatabaseInteractor);
        }

        /// <inheritdoc />
        public IOpenDMSDatabaseInteractor Handle(OracleDatabaseInteractor oracleDatabaseInteractor)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public IOpenDMSDatabaseInteractor Handle(SQLServerDatabaseInteractor sQLServerDatabaseInteractor)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public IOpenDMSDatabaseInteractor Handle(PostgreSQLDatabaseInteractor postgreSQLDatabaseInteractor)
        {
            return new DatabaseInteractorPostgreSQL(postgreSQLDatabaseInteractor);
        }
    }
}
