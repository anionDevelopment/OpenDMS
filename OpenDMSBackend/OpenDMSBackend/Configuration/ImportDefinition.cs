namespace OpenDMSBackend.Core.Configuration
{
    public class ImportDefinition
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public string SourceLocationURL { get; set; }
        public string TargetFolderId { get; set; }
        /// <example>
        /// document.name = "Document_" .. document.ReadableId .. "_" .. os.date("%Y-%m-%dT%H:%M:%S", document.import_date)
        /// document.businessowner = "OU73"
        /// </example>
        public string? AdaptDocumentScriptBody { get; set; }
    }
}
