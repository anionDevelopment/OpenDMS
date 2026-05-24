using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GRYLogger;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public sealed class DatabaseTestFrameworkForPostgreSQL : DatabaseTestFrameworkTemplate
    {
        public DatabaseTestFrameworkForPostgreSQL(IGRYLog log) : base(new DatabasePersistenceConfiguration() { DatabaseType = "PostgreSQL", DatabaseConnectionString = Utilities.GetTestPostgreSQLConnectionString() }, Utilities.GetTestPostgreSQLDatabaseFolder(), OpenDMSBackend.Tests.TestUtilities.Constants.GeneralConstants.RepositoryFolder, "LocaltestservicePostgresqlStart", "LocaltestservicePostgresqlStop", OpenDMSBackend.Tests.TestUtilities.Utilities.GetResetDatabaseScript("PostgreSQL"), log, TimeSpan.FromSeconds(200), new HashSet<string>() { "opendms_database" })
        {
        }

        public override string GetDatabaseTypeName()
        {
            return "PostgreSQL";
        }
    }
}
