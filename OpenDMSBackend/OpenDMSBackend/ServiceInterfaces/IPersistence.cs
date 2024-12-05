using GRYLibrary.Core.APIServer.Services;
using OpenDMSBackend.Core.Model;
using System;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public interface IPersistence : IExternalService
    {
        public void CreateDocument(Document document);
        public bool DocumentExists(string id);
        public uint GetAmountOfDocuments();
        public void Reset();
        public bool UserWithNameExists(string username);
        public bool UserWithIdExists(string userId);
        public ulong GetNewReadableId();
        public Document GetDocument(string id);
        void CreateTag(Tag tag);
        void AssignTag(string documentId, string tagId);
        void UnassignTag(string documentId, string tagId);
    }
}
