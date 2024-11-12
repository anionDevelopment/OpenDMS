using OpenDMSBackend.Core.Model;
using System.Collections.Generic;
using GRYLibrary.Core.APIServer.Services.Trans;
using System;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public sealed class TransientPersistence : IPersistence
    {
        private readonly IDictionary<string/*documentid*/, Document> _Documents;
        private readonly IAuthenticationServicePersistence<OpenDMSBackend.Core.Model.User> _TransientAuthenticationServicePersistence;

        public TransientPersistence(IAuthenticationServicePersistence<OpenDMSBackend.Core.Model.User> transientAuthenticationServicePersistence)
        {
            this._TransientAuthenticationServicePersistence = transientAuthenticationServicePersistence;
            this._Documents = new Dictionary<string, Document>();
            this.Initialize();
        }

        private void Initialize()
        {
            this.Reset();
        }


        public void Reset()
        {
            this._Documents.Clear();
        }

        public void CreateDocument(Document document)
        {
            throw new NotImplementedException();
        }

        public void DocumentExists(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool IsAvailable()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public uint GetAmountOfDocuments()
        {
           return (uint)_Documents.Count;
        }

        public bool UserExistsByName(string adminUserName)
        {
            throw new NotImplementedException();
        }

        public bool UserWithIdExists(string userId)
        {
            throw new NotImplementedException();
        }
    }
}