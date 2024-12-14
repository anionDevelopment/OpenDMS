using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    public interface IContainer
    {
        public ISet<IContainee> Content { get; set; }
        public void Accept(IContainerVisitor containee);
        public T Accept<T>(IContainerVisitor<T> containee);
    }
    public interface IContainerVisitor
    {
        void Handle(Folder folder);
        void Handle(StorageLocation storageLocation);
    }
    public interface IContainerVisitor<T>
    {
        T Handle(Folder folder);
        T Handle(StorageLocation storageLocation);
    }
}
