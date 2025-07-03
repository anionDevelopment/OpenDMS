using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Migration;
using Microsoft.EntityFrameworkCore;
using Moq;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Database;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenDMSBackendUtilities = OpenDMSBackend.Core.Miscellaneous.Utilities;
using System.Threading.Tasks;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public abstract class DatabasePersistenceTestsBase: PersistenceTestsBase
    {
        public abstract ISQLProvider GetSQLProvider();

        public abstract IDatabaseManager GetDatabaseManager();

        public abstract IPersistence GetPersistenceObject(IDatabaseManager databaseManager, ITimeService timeService, DbContextOptionsBuilder<DatabaseContext> optionsBuilder, IGRYLog logger, ISQLProvider sqlProvider);

        public abstract DatabaseTestFrameworkTemplate GetTestFramework();
        public override IPersistence GetPersistence()
        {
            DatabaseTestFrameworkTemplate databaseTestFramework = GetTestFramework();
            IDatabaseManager databaseManager = GetDatabaseManager();
            ITimeService timeService = new TimeService();
            GRYMigrator.DoAllMigrations(databaseTestFramework.Connection, databaseManager, timeService);
            DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseMySql(databaseTestFramework.ConnectionString, ServerVersion.AutoDetect(databaseTestFramework.ConnectionString));
            IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> persistedAPIServerConfiguration = new PersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>();
            persistedAPIServerConfiguration.ApplicationSpecificConfiguration = new CodeUnitSpecificConfiguration();
            persistedAPIServerConfiguration.ApplicationSpecificConfiguration.RegistrationIsEnabled = true;
            Mock<IIdGenerator<ulong>> idGeneratorMock = new Mock<IIdGenerator<ulong>>(MockBehavior.Strict);
            Mock<IExampleDataCreator> exampleDataCreatorMock = new Mock<IExampleDataCreator>(MockBehavior.Strict);
            exampleDataCreatorMock.Setup(mock => mock.AddExampleData());
            IGRYLog logger = GeneralLogger.CreateUsingConsole();
            Mock<IApplicationConstants<CodeUnitSpecificConstants>> constantsMock = new Mock<IApplicationConstants<CodeUnitSpecificConstants>>(MockBehavior.Strict);
            constantsMock.SetupGet(m => m.Environment).Returns(OpenDMSBackendUtilities.GetEnvironmentTargetType());
            ISQLProvider sqlProvider = GetSQLProvider();
            IPersistence result = GetPersistenceObject(databaseManager, timeService, optionsBuilder, logger, sqlProvider);
            return result;
        }

    }
}
