using GRYLibrary.Core.APIServer.Services;
using OpenDMSBackend.Core.Model;
using System;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public interface IPersistence : IExternalService
    {
        public void CreateDocument(Document document);
        public void DocumentExists(Guid id);
        uint GetAmountOfDocuments();
        void Reset();
        bool UserExistsByName(string adminUserName);
        bool UserWithIdExists(string userId);
    }
}
