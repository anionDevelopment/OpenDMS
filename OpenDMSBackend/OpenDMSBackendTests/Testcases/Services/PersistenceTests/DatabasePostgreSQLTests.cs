using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Database;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities;

namespace OpenDMSBackend.Tests.Testcases.Services.PersistenceTests
{
    [TestClass]
    public class DatabasePostgreSQLTests : DatabasePersistenceTestsBase
    {

        public override IDatabaseManager GetDatabaseManager()
        {
            return new DatabaseManagerPostgreSQL();
        }

        public override DatabasePersistence GetPersistenceObject(IDatabaseManager databaseManager, ITimeService timeService, IDatabasePersistenceConfiguration configuration, IGRYLog logger, ISQLProvider sqlProvider)
        {
            return new DatabasePostgreSQLPersistence(configuration, logger, timeService, databaseManager, logger, sqlProvider);
        }

        public override ISQLProvider GetSQLProvider(IGRYLog log)
        {
            return new SQLProviderPostgreSQL(log);
        }

        public override DatabaseTestFrameworkTemplate GetTestFramework()
        {
            return new DatabaseTestFrameworkForPostgreSQL();
        }


        [TestMethod(nameof(DatabasePersistenceCreateDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void DatabasePersistenceCreateDocumentTest()
        {
            this.DatabasePersistenceCreateDocument();
        }


        [TestMethod(nameof(GetAmountOfDocumentsTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void GetAmountOfDocumentsTest()
        {
            this.GetAmountOfDocuments();
        }

        [TestMethod(nameof(PersistDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void PersistDocumentTest()
        {
            this.PersistDocument();
        }
    }
}
