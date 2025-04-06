using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.Misc.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Tests.Testcases.Services
{
    public abstract class PersistenceTests
    {
        public abstract void PersistDocumentTest();
        public void PersistDocumentTest(IPersistence persistence, ITimeService timeService)
        {
            //arrange
            Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), timeService.GetCurrentTimeAsGRYDateTime(), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 });
            Assert.IsFalse(persistence.IsDocument(testDocument.Id));

            //act
            persistence.CreateDocument(testDocument);

            //assert
            Assert.IsTrue(persistence.IsDocument(testDocument.Id));
        }
    }
}
