using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Services;
using IdGenerator = OpenDMSBackend.Core.Services.IdGenerator;
using OpenDMSBackend.Core.Model.BusinessTypes;

namespace OpenDMSBackend.Tests.Testcases.Services
{
    [TestClass]
    public class TransientPersistenceTests : PersistenceTests
    {
        private (TransientPersistence persistence, ITimeService timeService) GetTransientPersistence()
        {
            ITimeService timeService = new TimeService();
            IIdGenerator<ulong> idGenerator = new IdGenerator();
            TransientAuthenticationServicePersistence<User> transientAuthenticationServicePersistence = new TransientAuthenticationServicePersistence(timeService);
            TransientPersistence persistence = new TransientPersistence(transientAuthenticationServicePersistence, idGenerator);
            return (persistence, timeService);
        }

        [TestMethod(nameof(PersistDocumentTest))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public override void PersistDocumentTest()
        {
            (TransientPersistence persistence, ITimeService timeService) = this.GetTransientPersistence();
            this.PersistDocumentTest(persistence, timeService);
        }
    }
}
