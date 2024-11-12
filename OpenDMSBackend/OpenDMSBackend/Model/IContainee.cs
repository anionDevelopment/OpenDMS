namespace OpenDMSBackend.Core.Model
{
    public interface IContainee
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
