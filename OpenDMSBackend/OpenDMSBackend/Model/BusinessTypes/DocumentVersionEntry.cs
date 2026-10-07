using System;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    /// <summary>One entry of the <c>DocumentVersion</c>-table: it assigns one version (a row in the <c>Documents</c>-table, referenced by <see cref="ContentId"/>) to a logical document (<see cref="DocumentId"/>).</summary>
    public class DocumentVersionEntry
    {
        /// <summary>The id of the logical document all versions of a version-chain share.</summary>
        public string DocumentId { get; set; }
        /// <summary>The id of the <c>Documents</c>-row (the version's content and metadata) this entry refers to.</summary>
        public string ContentId { get; set; }
        /// <summary>The incrementing (1-based) version-number.</summary>
        public int Version { get; set; }
        /// <summary>The moment this version was created.</summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>Initializes a new instance of <see cref="DocumentVersionEntry"/>.</summary>
        /// <param name="documentId">The logical document-id.</param>
        /// <param name="contentId">The id of the referenced <c>Documents</c>-row.</param>
        /// <param name="version">The version-number.</param>
        /// <param name="timestamp">The moment this version was created.</param>
        public DocumentVersionEntry(string documentId, string contentId, int version, DateTimeOffset timestamp)
        {
            this.DocumentId = documentId;
            this.ContentId = contentId;
            this.Version = version;
            this.Timestamp = timestamp;
        }
    }
}
