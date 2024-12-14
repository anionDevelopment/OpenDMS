using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    public class Folder : IFolder
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

        public void Accept(IContaineeVisitor containee)
        {
            containee.Handle(this);
        }

        public T Accept<T>(IContaineeVisitor<T> containee)
        {
            return containee.Handle(this);
        }
    }
}
