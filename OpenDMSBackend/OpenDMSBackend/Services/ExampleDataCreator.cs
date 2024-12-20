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
            (string userId, string storageLocationId) userDetails1 = this.AddUser(1);
            this.AddExampleDocument(1, userDetails1.userId, userDetails1.storageLocationId);

            (string userId, string storageLocationId) userDetails2 = this.AddUser(2);
            this.AddExampleDocument(2, userDetails2.userId, userDetails2.storageLocationId);
            this.AddExampleDocument(3, userDetails2.userId, userDetails2.storageLocationId);
        }

        private (string userId, string storageLocationId) AddUser(int userNumber)
        {
            string userName = $"user{userNumber.ToString().PadLeft(2, '0')}";
            string password = userName;
            string userId = this._BusinessLogicService.Register(userName, password);
            string storageLocationId = this._BusinessLogicService.AddStorageLocation(userId, "MainStorage");
            return (userId, storageLocationId);
        }

        private void AddExampleDocument(int documentNumber, string ownerId, string locationId)
        {
            string title = $"Document{documentNumber.ToString().PadLeft(2, '0')}";
            string filename = $"{title}.pdf";
            byte[] content = GetFileContentFromEmbeddedExampleDocuments(filename);
            this._BusinessLogicService.AddDocument(ownerId, title, locationId, filename, content);
        }

        public static byte[] GetFileContentFromEmbeddedExampleDocuments(string documentName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"OpenDMSBackend.Core.Resources.ExampleDocuments.{documentName}";
            using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
            BinaryReader binaryReader = new BinaryReader(stream);
            using BinaryReader reader = binaryReader;
            var result= reader.ReadBytes((int)stream.Length);
            return result;
        }
    }
}
