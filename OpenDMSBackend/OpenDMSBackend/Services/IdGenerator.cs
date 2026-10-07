using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Services
{
    public class IdGenerator : IIdGenerator<ulong>
    {
        private ulong _LastValue;

        /// <summary>Initializes a new instance of <see cref="IdGenerator"/> starting at zero.</summary>
        public IdGenerator()
        {
        }

        /// <inheritdoc />
        public ISet<ulong> GeneratedIds()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public ulong GenerateNewId()
        {
            this._LastValue = this._LastValue + 1;
            return this._LastValue;
        }

        /// <inheritdoc />
        public void Reset()
        {
            this._LastValue = 0;
        }

        /// <inheritdoc />
        public void Reset(ulong lastValue)
        {
            this._LastValue = lastValue;
        }
    }
}
