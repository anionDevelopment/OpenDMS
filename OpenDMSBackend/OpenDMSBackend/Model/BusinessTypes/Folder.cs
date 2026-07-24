using OpenDMSBackend.Core.Model.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    public class Folder : IFolder
    {
        public string Id { get; set; }
        public ISet<IContainee> Content { get; set; } = new HashSet<IContainee>();
        public string Name { get; set; }

        /// <inheritdoc />
        public void Accept(IContainerVisitor containee)
        {
            containee.Handle(this);
        }

        /// <inheritdoc />
        public T Accept<T>(IContainerVisitor<T> containee)
        {
            return containee.Handle(this);
        }

        /// <inheritdoc />
        public void Accept(IContaineeVisitor containee)
        {
            containee.Handle(this);
        }

        /// <inheritdoc />
        public T Accept<T>(IContaineeVisitor<T> containee)
        {
            return containee.Handle(this);
        }

        /// <summary>Converts this instance to its DTO representation.</summary>
        /// <returns>A <see cref="FolderDTO"/> containing folder id, name, and child ids.</returns>
        public FolderDTO ToDTO()
        {
            return new FolderDTO(this.Id, this.Name, this.Content.Where(containee => containee is Folder).Select(folder => folder.Id), this.Content.Where(containee => containee is Document).Select(document => document.Id));
        }
    }
}
