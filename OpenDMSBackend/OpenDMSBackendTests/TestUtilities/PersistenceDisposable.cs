using GRYLibrary.Core.Misc.CustomDisposables;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Tests.TestUtilities
{
    internal class PersistenceDisposable : CustomDisposable
    {
        public IPersistence Persistence { get; private set; }
        public PersistenceDisposable(IPersistence persistence, ISet<IDisposable> disposables) : base(() =>
        {
            persistence.Dispose();
            foreach (IDisposable disposable in disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    throw;
                }
            }
        })
        {
            this.Persistence = persistence;
        }
    }
}
