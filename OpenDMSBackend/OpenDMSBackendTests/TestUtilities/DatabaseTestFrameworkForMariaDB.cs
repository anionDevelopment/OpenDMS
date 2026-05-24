using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GRYLogger;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFrameworkForMariaDB : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFrameworkForMariaDB(IGRYLog log) : base(new DatabasePersistenceConfiguration() { DatabaseType = "MariaDB", DatabaseConnectionString = Utilities.GetTestMariaDBConnectionString() }, Utilities.GetTestMariaDBDatabaseFolder(), OpenDMSBackend.Tests.TestUtilities.Constants.GeneralConstants.RepositoryFolder, "LocaltestserviceMariadbStart", "LocaltestserviceMariadbStop", OpenDMSBackend.Tests.TestUtilities.Utilities.GetResetDatabaseScript("MariaDB"), log,TimeSpan.FromSeconds(200), new HashSet<string>() { "opendms_database" })
        {
        }

        public override string GetDatabaseTypeName()
        {
            return "MariaDB";
        }
    }
}
