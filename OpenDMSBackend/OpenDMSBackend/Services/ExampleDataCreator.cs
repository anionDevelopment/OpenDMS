using GRYLibrary.Core.Misc;
using System;
using System.IO;
using System.Reflection;

namespace OpenDMSBackend.Core.Services
{
    public class ExampleDataCreator : IExampleDataCreator
    {
        private readonly IBusinessLogicService _BusinessLogicService;

        public ExampleDataCreator(IBusinessLogicService businessLogicService)
        {
            this._BusinessLogicService = businessLogicService;
        }

        public void AddExampleData()
        {
            DateTime now = DateTime.Now;
            DateTime initialDate = new DateTime(now.Year,now.Month,now.Day,now.Hour,0,0).AddHours(-1);
            (string userId, string storageLocationId) userDetails1 = this.AddUser(1);//minimal example
            this.AddExampleDocument(1, userDetails1.userId, userDetails1.storageLocationId, initialDate);

            (string userId, string storageLocationId) userDetails2 = this.AddUser(2);//example with some more documents
            string folder1Id = this.AddFolder(1, userDetails2.userId, userDetails2.storageLocationId);
            string folder2Id = this.AddFolder(2, userDetails2.userId, userDetails2.storageLocationId);
            string folder3Id = this.AddFolder(3, userDetails2.userId, userDetails2.storageLocationId);
            string folder4Id = this.AddFolder(4, userDetails2.userId, folder3Id);
            string folder5Id = this.AddFolder(5, userDetails2.userId, userDetails2.storageLocationId);
            this.AddExampleDocument(2, userDetails2.userId, folder2Id, initialDate);
            this.AddExampleDocument(3, userDetails2.userId, folder2Id, initialDate);
            this.AddExampleDocument(4, userDetails2.userId, folder4Id, initialDate);
            this.AddExampleDocument(5, userDetails2.userId, folder3Id, initialDate);
            this.AddExampleDocument(6, userDetails2.userId, folder5Id, initialDate);
            this.AddExampleDocument(7, userDetails2.userId, folder5Id, initialDate);
            this.AddExampleDocument(8, userDetails2.userId, userDetails2.storageLocationId, initialDate);
            this.AddExampleDocument(9, userDetails2.userId, userDetails2.storageLocationId, initialDate);
            this.AddExampleDocument(10, userDetails2.userId, userDetails2.storageLocationId, initialDate);
            /* Structure for user02:
                MainStorage
                ├─ folder01/
                ├─ folder02/
                │  ├─ file02
                │  └─ file03
                ├─ folder03/
                │  ├─ folder04/
                │  │  └─ file04
                │  └─ file05
                ├─ folder05/
                │  ├─ file06
                │  └─ file07
                ├─ file08
                ├─ file09
                └─ file10
             */
        }

        private string AddFolder(int folderId, string userId, string container)
        {
            string folderName = $"Folder{folderId.ToString().PadLeft(2, '0')}";
            return this._BusinessLogicService.AddFolder(userId, folderName, container);
        }

        private (string userId, string storageLocationId) AddUser(int userNumber)
        {
            string userName = $"user{userNumber.ToString().PadLeft(2, '0')}";
            string password = userName;
            string userId = this._BusinessLogicService.Register(userName, password);
            string storageLocationId = this._BusinessLogicService.AddStorageLocation(userId, $"MainStorage of {userName}");
            return (userId, storageLocationId);
        }

        private void AddExampleDocument(int documentNumber, string ownerId, string locationId, DateTime initialDate)
        {
            string title = $"Document{documentNumber.ToString().PadLeft(2, '0')}";
            string filename = $"{title}.pdf";
            byte[] content = GetFileContentFromEmbeddedExampleDocuments(filename);
            DateTime creationDate = initialDate.AddMinutes(documentNumber);
            this._BusinessLogicService.AddDocument(ownerId, title, locationId, filename, content, GRYDateTime.FromDateTime(creationDate));
        }

        public static byte[] GetFileContentFromEmbeddedExampleDocuments(string documentName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"OpenDMSBackend.Core.Resources.ExampleDocuments.{documentName}";
            using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
            BinaryReader binaryReader = new BinaryReader(stream);
            using BinaryReader reader = binaryReader;
            byte[] result = reader.ReadBytes((int)stream.Length);
            return result;
        }
    }
}
