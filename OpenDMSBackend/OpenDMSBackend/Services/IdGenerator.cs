using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Services
{
    public class IdGenerator : IIdGenerator<ulong>
    {
        private readonly IPersistence _Persistence;
        
        public IdGenerator(IPersistence persistence)
        {
            this._Persistence = persistence;
        }

        public ISet<ulong> GeneratedIds()
        {
            throw new NotSupportedException();
        }

        public ulong GenerateNewId()
        {
           return this._Persistence.GetLatestReadableId()+1;
        }

        public void Reset()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }
    }
}
