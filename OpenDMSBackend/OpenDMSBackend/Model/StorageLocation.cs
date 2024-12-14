using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model
{
    public class StorageLocation : IStorageLocation
    {
        public ISet<IContainee> Content { get; set; } = new HashSet<IContainee>();
        public void Accept(IContainerVisitor containee)
        {
            containee.Handle(this);
        }

        public T Accept<T>(IContainerVisitor<T> containee)
        {
            return containee.Handle(this);
        }
    }
}
