using OpenDMSBackend.Core.Model;
using System.Collections.Generic;
using GRYLibrary.Core.APIServer.Services.Trans;
using System;
using System.Linq;
using GRYLibrary.Core.Misc;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public sealed class TransientPersistence : IPersistence
    {
        private readonly IDictionary<string/*id*/, Document> _Documents;
        private readonly IDictionary<string/*id*/, Model.User> _Users;
        private readonly IDictionary<string/*id*/, Tag> _Tags;
        private readonly IIdGenerator<ulong> _IdGenerator;
        private readonly IAuthenticationServicePersistence<OpenDMSBackend.Core.Model.User> _TransientAuthenticationServicePersistence;

        public TransientPersistence(IAuthenticationServicePersistence<OpenDMSBackend.Core.Model.User> transientAuthenticationServicePersistence)
        {
            this._TransientAuthenticationServicePersistence = transientAuthenticationServicePersistence;
            this._Documents = new Dictionary<string, Document>();
            this._Users = new Dictionary<string, User>();
            this._Tags = new Dictionary<string, Tag>();
            _IdGenerator = IdGenerator.GetDefaultLongIdGenerator();
            this.Initialize();
        }

        private void Initialize()
        {
            this.Reset();
        }


        public void Reset()
        {
            this._Documents.Clear();
            this._Users.Clear();
            this._Tags.Clear();
            this._IdGenerator.Reset();
        }

        public void CreateDocument(Document document)
        {
            _Documents[document.Id] = document;
        }

        public bool DocumentExists(string id)
        {
            return _Documents.ContainsKey(id);
        }

        public bool IsAvailable()
        {
            return true;
        }

        public void Dispose()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }

        public uint GetAmountOfDocuments()
        {
            return (uint)this._Documents.Count;
        }

        public bool UserWithNameExists(string username)
        {
            return _Users.Where(u => u.Value.Name == username).Any();
        }

        public bool UserWithIdExists(string userId)
        {
            return _Users.ContainsKey(userId);
        }

        public ulong GetNewReadableId()
        {
            return _IdGenerator.GenerateNewId();
        }

        public Document GetDocument(string id)
        {
            return _Documents[id];
        }

        public void CreateTag(Tag tag)
        {
            _Tags[tag.Id] = tag;
        }
        private Tag GetTag(string id)
        {
            return _Tags[id];
        }

        public void AssignTag(string documentId, string tagId)
        {
            GetDocument(documentId).Tags.Add(GetTag(tagId));
        }

        public void UnassignTag(string documentId, string tagId)
        {
            GetDocument(documentId).Tags.Remove(GetTag(tagId));
        }
    }
}