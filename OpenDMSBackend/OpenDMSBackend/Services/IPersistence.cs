using GRYLibrary.Core.APIServer.Services;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;

namespace OpenDMSBackend.Core.Services
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
        public void CreateTag(Tag tag);
        public void AssignTag(string documentId, string tagId);
        public void UnassignTag(string documentId, string tagId);
        public TagDTO[] GetAllTags();
    }
}
