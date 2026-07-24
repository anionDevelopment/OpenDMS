using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.DTOs
{
    public class StorageLocationDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> ContainedFolderIds { get; set; }
        public IEnumerable<string> ContainedDocumentIds { get; set; }

        public StorageLocationDTO(string id, string name, IEnumerable<string> containedFolderIds, IEnumerable<string> containedDocumentIds)
        {
            this.Id = id;
            this.Name = name;
            this.ContainedFolderIds = containedFolderIds;
            this.ContainedDocumentIds = containedDocumentIds;
        }
    }
}
