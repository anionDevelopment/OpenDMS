using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Tests.Testcases.Services.PersistenceTests
{
    [TestClass]
    public class PersistenceTestsForTransient : PersistenceTestsBase
    {
        internal override PersistenceDisposable GetPersistence(ITimeService timeService)
        {
            (TransientPersistence, ISet<IDisposable>) result = TestUtilities.Utilities.GetTransientPersistence(timeService);
            return new PersistenceDisposable(result.Item1, result.Item2);
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
