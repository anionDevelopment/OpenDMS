namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    /// <summary>The value-type of a storage-location-specific custom metadata-field (see issue #2 / #14). A document can optionally hold a value of this type for a field.</summary>
    public enum MetadataFieldType
    {
        /// <summary>A free-text (string) value.</summary>
        String,
        /// <summary>A boolean value (for example "tax-relevant" yes/no).</summary>
        Boolean
    }
}
