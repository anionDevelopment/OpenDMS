using OpenDMSBackend.Core.Model.DTOs;
using System;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// The definition of a custom metadata-field which a moderator of a storage-location has defined for that storage-location.
    /// Every document contained in the storage-location (directly or in one of its folders) can optionally hold a value of the field's <see cref="Type"/>.
    /// </summary>
    public class MetadataFieldDefinition
    {
        /// <summary>The unique identifier of the field-definition.</summary>
        public string Id { get; set; }
        /// <summary>The id of the storage-location this field is defined for.</summary>
        public string StorageLocationId { get; set; }
        /// <summary>The display-name of the field (unique within the storage-location).</summary>
        public string Name { get; set; }
        /// <summary>The value-type of the field.</summary>
        public MetadataFieldType Type { get; set; }

        /// <summary>Initializes a new instance of <see cref="MetadataFieldDefinition"/>.</summary>
        /// <param name="id">The unique identifier of the field-definition.</param>
        /// <param name="storageLocationId">The id of the storage-location this field is defined for.</param>
        /// <param name="name">The display-name of the field.</param>
        /// <param name="type">The value-type of the field.</param>
        public MetadataFieldDefinition(string id, string storageLocationId, string name, MetadataFieldType type)
        {
            this.Id = id;
            this.StorageLocationId = storageLocationId;
            this.Name = name;
            this.Type = type;
        }

        /// <summary>Converts this instance to its DTO representation.</summary>
        /// <returns>A <see cref="MetadataFieldDefinitionDTO"/> representing this field-definition.</returns>
        public MetadataFieldDefinitionDTO ToDTO()
        {
            return new MetadataFieldDefinitionDTO(this.Id, this.StorageLocationId, this.Name, this.Type.ToString());
        }

        public override bool Equals(object? obj)
        {
            return obj is MetadataFieldDefinition other && this.Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Id);
        }
    }
}
