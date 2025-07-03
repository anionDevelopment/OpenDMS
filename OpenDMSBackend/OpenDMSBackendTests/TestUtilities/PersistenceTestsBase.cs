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
using GRYLibrary.Core.Misc.Strings;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Database;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using OpenDMSBackendUtilities = OpenDMSBackend.Core.Miscellaneous.Utilities;

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
            Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new GRYLibrary.Core.Misc.GRYDateTime(2025, 07, 03, 21, 0, 6), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 });
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
            IPersistence databasePersistence = GetPersistence();

            Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new GRYLibrary.Core.Misc.GRYDateTime(2025, 07, 03, 21, 0, 6), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 });

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
            IPersistence databasePersistence = GetPersistence();
            Document testDocument = new Document(Guid.NewGuid().ToString(), OneLineString.From("title"), OneLineString.From("Filename.pdf"), OneLineString.From($"Originalfilename_{Guid.NewGuid()}.pdf"), new GRYLibrary.Core.Misc.GRYDateTime(2025, 07, 03, 21, 0, 6), default, 1, new HashSet<Tag>(), OneLineString.From("application/pdf"), new byte[] { 1, 2, 3, 4 }, string.Empty, new byte[] { 1, 2 });

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
