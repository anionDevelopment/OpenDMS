using GRYLibrary.Core.APIServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OpenDMSBackend.Core.Services
{
    public class ExampleDataCreator : IExampleDataCreator
    {
        private readonly IBusinessLogicService _BusinessLogicService;
        private readonly IAuthenticationService<OpenDMSBackend.Core.Model.BusinessTypes.User> _AuthenticationService;

        public ExampleDataCreator(IBusinessLogicService businessLogicService,IAuthenticationService<OpenDMSBackend.Core.Model.BusinessTypes.User> authenticationService)
        {
            this._BusinessLogicService = businessLogicService;
            this._AuthenticationService = authenticationService;
        }

        public void AddExampleData()
        {
            DateTime now = DateTime.Now;
            DateTime initialDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(-1);
            (string userId, string storageLocationId) userDetails1 = this.AddUser(1);//minimal example
            string userGroupe = this._AuthenticationService.GetBaseRoleOfAllUser();

            (string userId, string storageLocationId) userDetails2 = this.AddUser(2);//example with some more documents
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            string folder1Id = this.AddFolder(1, userDetails2.userId, userDetails2.storageLocationId);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            string folder2Id = this.AddFolder(2, userDetails2.userId, userDetails2.storageLocationId);
            string folder3Id = this.AddFolder(3, userDetails2.userId, userDetails2.storageLocationId);
            string folder4Id = this.AddFolder(4, userDetails2.userId, folder3Id);
            string folder5Id = this.AddFolder(5, userDetails2.userId, userDetails2.storageLocationId);
            this.AddExampleDocument(1, userDetails1.userId, userDetails1.storageLocationId, initialDate, userGroupe);
            this.AddExampleDocument(2, userDetails2.userId, folder2Id, initialDate, userGroupe);
            this.AddExampleDocument(3, userDetails2.userId, folder2Id, initialDate, userGroupe);
            this.AddExampleDocument(4, userDetails2.userId, folder4Id, initialDate, userGroupe);
            this.AddExampleDocument(5, userDetails2.userId, folder3Id, initialDate, userGroupe);
            this.AddExampleDocument(6, userDetails2.userId, folder5Id, initialDate, userGroupe);
            this.AddExampleDocument(7, userDetails2.userId, folder5Id, initialDate, userGroupe);
            this.AddExampleDocument(8, userDetails2.userId, userDetails2.storageLocationId, initialDate, userGroupe);
            this.AddExampleDocument(9, userDetails2.userId, userDetails2.storageLocationId, initialDate, userGroupe);
            this.AddExampleDocument(10, userDetails2.userId, userDetails2.storageLocationId, initialDate, userGroupe);
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

        private void AddExampleDocument(int documentNumber, string ownerId, string locationId, DateTime initialDate, string groupOfBusinessOwner)
        {
            string title = $"Document{documentNumber.ToString().PadLeft(2, '0')}";
            string filename = $"{title}.pdf";
            byte[] content = GetFileContentFromEmbeddedExampleDocuments(filename);
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            DateTime creationDate = initialDate.AddMinutes(documentNumber);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            this._BusinessLogicService.AddDocument(ownerId, title, locationId, filename, content,  groupOfBusinessOwner,new HashSet<string>());
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
