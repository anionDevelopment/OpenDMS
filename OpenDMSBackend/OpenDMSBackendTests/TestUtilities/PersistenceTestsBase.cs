using GRYLibrary.Core.Misc.Strings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public abstract class PersistenceTestsBase
    {
        public PersistenceTestsBase()
        {
        }

        public abstract IPersistence GetPersistence();


        public abstract void PersistDocumentTest();
        public void PersistDocument()
        {
            //arrange
            using IPersistence persistence = this.GetPersistence();
            Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 01, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0),new HashSet<string>());
            Assert.IsFalse(persistence.IsDocument(testDocument.Id));

            //act
            persistence.CreateDocument(testDocument);

            //assert
            Assert.IsTrue(persistence.IsDocument(testDocument.Id));
        }

        public abstract void DatabasePersistenceCreateDocumentTest();
        public void DatabasePersistenceCreateDocument()
        {
            //arrange          
            IPersistence databasePersistence = this.GetPersistence();

            Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 02, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0),new HashSet<string>());

            //act
            databasePersistence.CreateDocument(testDocument);

            // assert
            Document reloadedDocument = databasePersistence.GetDocument(testDocument.Id);
            Assert.AreEqual(testDocument.OriginalFilename.Value, reloadedDocument.OriginalFilename.Value);
        }

        public abstract void GetAmountOfDocumentsTest();
        public void GetAmountOfDocuments()
        {
            //arrange
            IPersistence databasePersistence = this.GetPersistence();
            Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new DateTimeOffset(2025, 08, 06, 20, 00, 03, TimeSpan.Zero), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 }, false, default, default, CodeUnitSpecificConstants.RolenameUsers, new GRYLibrary.Core.Misc.Version3(1, 0, 0), new HashSet<string>());

            uint expectedAmount1 = 0;
            uint expectedAmount2 = 1;

            //act
            uint actualAmount1 = databasePersistence.GetAmountOfDocuments();
            databasePersistence.CreateDocument(testDocument);
            uint actualAmount2 = databasePersistence.GetAmountOfDocuments();

            // assert

            Assert.AreEqual(expectedAmount1, actualAmount1);
            Assert.AreEqual(expectedAmount2, actualAmount2);
        }


    }
}
