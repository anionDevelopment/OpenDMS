using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Services
{
    public class IdGenerator : IIdGenerator<ulong>
    {
        private ulong _LastValue;

        public IdGenerator()
        {
        }

        public ISet<ulong> GeneratedIds()
        {
            throw new NotSupportedException();
        }

        public ulong GenerateNewId()
        {
            this._LastValue = this._LastValue + 1;
            return this._LastValue;
        }

        public void Reset()
        {
            this._LastValue = 0;
        }

        public void Reset(ulong lastValue)
        {
            this._LastValue = lastValue;
        }
    }
}
