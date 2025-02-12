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
            _LastValue = _LastValue + 1;
            return _LastValue;
        }

        public void Reset()
        {
            _LastValue = 0;
        }

        public void Reset(ulong lastValue)
        {
            _LastValue = lastValue;
        }
    }
}
