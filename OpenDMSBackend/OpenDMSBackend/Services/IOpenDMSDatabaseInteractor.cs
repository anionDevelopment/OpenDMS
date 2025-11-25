using GRYLibrary.Core.APIServer.Services.Database;

namespace OpenDMSBackend.Core.Services
{
    public interface IOpenDMSDatabaseInteractor : IProjectSpecificDatabaseInteractor
    {
        public ISQLProvider GetSQLProvider();
    }
}
