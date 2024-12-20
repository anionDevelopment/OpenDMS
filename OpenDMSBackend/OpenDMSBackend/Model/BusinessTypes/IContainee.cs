using OpenDMSBackend.Core.Services;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    public interface IContainee: IContent
    {
        public void Accept(IContaineeVisitor containee);
        public T Accept<T>(IContaineeVisitor<T> containee);
    }
    public interface IContaineeVisitor
    {
        void Handle(Folder folder);
        void Handle(Document document);
    }
    public interface IContaineeVisitor<T>
    {
        T Handle(Folder folder);
        T Handle(Document document);
    }
}
