namespace OpenDMSBackend.Core.Model.DTOs
{
    /// <summary>Data transfer object used to create a new custom metadata-field-definition for a storage-location.</summary>
    public class MetadataFieldDefinitionCreationDTO
    {
        /// <summary>The display-name of the field to create.</summary>
        public string Name { get; set; }
        /// <summary>The value-type of the field ("String" or "Boolean").</summary>
        public string Type { get; set; }

        /// <summary>Initializes a new instance of <see cref="MetadataFieldDefinitionCreationDTO"/>.</summary>
        /// <param name="name">The display-name of the field to create.</param>
        /// <param name="type">The value-type of the field ("String" or "Boolean").</param>
        public MetadataFieldDefinitionCreationDTO(string name, string type)
        {
            this.Name = name;
            this.Type = type;
        }
    }
}
