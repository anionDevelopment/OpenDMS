using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.Misc.Migration;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Services
{
    public interface IOpenDMSDatabaseInteractor 
    {
        public IGenericDatabaseInteractor GetGenericDatabaseInteractor();
        public ISQLProvider GetSQLProvider();
        public IList<MigrationInstance> GetAllMigrations();
        public void SetLogConnectionAttemptErrors(bool enabled);
    }
}
