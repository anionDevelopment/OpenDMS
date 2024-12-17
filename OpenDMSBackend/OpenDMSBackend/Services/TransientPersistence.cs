using System;
using System.Collections.Generic;
using System.Linq;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;

namespace OpenDMSBackend.Core.Services
{
    public sealed class TransientPersistence : IPersistence
    {
        private readonly IDictionary<string/*id*/, Document> _Documents;
        private readonly IDictionary<string/*id*/, Tag> _Tags;
        private readonly IIdGenerator<ulong> _IdGenerator;
        private readonly IAuthenticationServicePersistence<User> _TransientAuthenticationServicePersistence;

        public TransientPersistence(IAuthenticationServicePersistence<User> transientAuthenticationServicePersistence)
        {
            this._TransientAuthenticationServicePersistence = transientAuthenticationServicePersistence;
            this._Documents = new Dictionary<string, Document>();
            this._Tags = new Dictionary<string, Tag>();
            this._IdGenerator = IdGenerator.GetDefaultLongIdGenerator();
            this.Initialize();
        }

        private void Initialize()
        {
            this.Reset();
        }

        public void Reset()
        {
            this._Documents.Clear();
            this._Tags.Clear();
            this._IdGenerator.Reset();
        }

        public void CreateDocument(Document document)
        {
            this._Documents[document.Id] = document;
        }

        public bool DocumentExists(string id)
        {
            return this._Documents.ContainsKey(id);
        }

        public bool IsAvailable()
        {
            return true;
        }

        public void Dispose()
        {
            Utilities.NoOperation();
        }

        public uint GetAmountOfDocuments()
        {
            return (uint)this._Documents.Count;
        }

        public bool UserWithNameExists(string username)
        {
            return this._TransientAuthenticationServicePersistence.UserWithNameExists(username);
        }

        public bool UserWithIdExists(string userId)
        {
            return this._TransientAuthenticationServicePersistence.UserWithIdExists(userId);
        }

        public ulong GetNewReadableId()
        {
            return (ulong)this._IdGenerator.GenerateNewId();
        }

        public Document GetDocument(string id)
        {
            return this._Documents[id];
        }

        public void CreateTag(Tag tag)
        {
            this._Tags[tag.Id] = tag;
        }
        private Tag GetTag(string id)
        {
            return this._Tags[id];
        }

        public void AssignTag(string documentId, string tagId)
        {
            this.GetDocument(documentId).Tags.Add(this.GetTag(tagId));
        }

        public void UnassignTag(string documentId, string tagId)
        {
            this.GetDocument(documentId).Tags.Remove(this.GetTag(tagId));
        }

        public TagDTO[] GetAllTags()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<DocumentPreview> GetLatestDocuments(string userId, uint maximalAmountOfDocuments)
        {
            return _Documents.Values.Where(document => document.OwnerId == userId).OrderByDescending(document => document.LastEditDate).Take((int)maximalAmountOfDocuments).Select(d=>d.GetPreview());
        }
    }
}