namespace OpenDMSBackend.Core.Model.DTOs
{
    /// <summary>Data transfer object for a storage-location-specific custom metadata-field-definition.</summary>
    public class MetadataFieldDefinitionDTO
    {
        public string Id { get; set; }
        public string StorageLocationId { get; set; }
        public string Name { get; set; }
        /// <summary>The value-type of the field ("String" or "Boolean").</summary>
        public string Type { get; set; }

        /// <summary>Initializes a new instance of <see cref="MetadataFieldDefinitionDTO"/>.</summary>
        /// <param name="id">The unique identifier of the field-definition.</param>
        /// <param name="storageLocationId">The id of the storage-location this field is defined for.</param>
        /// <param name="name">The display-name of the field.</param>
        /// <param name="type">The value-type of the field ("String" or "Boolean").</param>
        public MetadataFieldDefinitionDTO(string id, string storageLocationId, string name, string type)
        {
            this.Id = id;
            this.StorageLocationId = storageLocationId;
            this.Name = name;
            this.Type = type;
        }
    }
}
