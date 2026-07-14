using GRYLibrary.Core.Misc;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.DTOs
{
    /// <summary>Represents a data transfer object for a full document, including its content.</summary>
    public class DocumentDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Filename { get; set; }
        public string OriginalFilename { get; set; }
        public string ImportDate { get; set; }
        public ISet<TagDTO> Tags { get; set; }
        public ulong ReadableId { get; set; }
        public string MimeType { get; set; }
        public string DocumentContentAsBase64 { get; set; }
        public string DocumentPreviewAsBase64 { get; set; }
        public bool IsSoftDeleted { get; set; }
        public string? DeleteIsNotAllowedBefore { get; set; }
        public string? MustBeHardDeletedAfter { get; set; }
        public string GroupOfBusinessOwner { get; set; }
        public ISet<string> AssignedLanguages { get; set; }

        public string? AddedByUserId { get; set; }

        /// <summary>A very short AI-generated summary (at most three sentences), or <see langword="null"/> if none has been generated yet.</summary>
        public string? AISummaryShort { get; set; }
        /// <summary>A regular AI-generated summary, or <see langword="null"/> if none has been generated yet.</summary>
        public string? AISummaryLong { get; set; }
        /// <summary>The incrementing version-number of this document within its version-chain (1-based).</summary>
        public int VersionNumber { get; set; }
        /// <summary>The moment this version was created, as a formatted string.</summary>
        public string? VersionTimestamp { get; set; }
        /// <summary>
        /// The values this document holds for the custom metadata-fields defined at its containing storage-location, keyed by the field-definition-id.
        /// A boolean-value is represented as "true"/"false". A field for which the document has no value is absent from this dictionary.
        /// </summary>
        public IDictionary<string, string> MetadataValues { get; set; } = new Dictionary<string, string>();

        /// <summary>Initializes a new instance of <see cref="DocumentDTO"/>.</summary>
        /// <param name="id">The unique identifier of the document.</param>
        /// <param name="title">The display title of the document.</param>
        /// <param name="filename">The stored filename of the document.</param>
        /// <param name="originalFilename">The original filename as it was at import time.</param>
        /// <param name="importDate">The date the document was imported, as a formatted string.</param>
        /// <param name="tags">The set of tags assigned to the document.</param>
        /// <param name="readableId">The human-readable numeric identifier of the document.</param>
        /// <param name="mimeType">The MIME type of the document content.</param>
        /// <param name="documentContentAsBase64">The full document content encoded as a Base64 string.</param>
        /// <param name="documentPreviewAsBase64">A preview image of the document encoded as a Base64 string.</param>
        /// <param name="isSoftDeleted">Whether the document has been soft-deleted.</param>
        /// <param name="deleteIsNotAllowedBefore">The earliest date on which deletion is permitted, or <see langword="null"/> if unrestricted.</param>
        /// <param name="mustBeHardDeletedAfter">The date after which the document must be hard-deleted, or <see langword="null"/> if not set.</param>
        /// <param name="groupOfBusinessOwner">The business-owner group associated with the document.</param>
        /// <param name="assignedLanguages">The set of language codes assigned to the document.</param>
        /// <param name="addedByUserId">The identifier of the user who added the document, or <see langword="null"/> if unknown.</param>
        public DocumentDTO(string id, string title, string filename, string originalFilename, string importDate, ISet<TagDTO> tags, ulong readableId, string mimeType, string documentContentAsBase64, string documentPreviewAsBase64, bool isSoftDeleted, string? deleteIsNotAllowedBefore, string? mustBeHardDeletedAfter, string groupOfBusinessOwner, ISet<string> assignedLanguages, string? addedByUserId)
        {
            this.Id = id;
            this.Title = title;
            this.Filename = filename;
            this.OriginalFilename = originalFilename;
            this.ImportDate = importDate;
            this.Tags = tags;
            this.ReadableId = readableId;
            this.MimeType = mimeType;
            this.DocumentContentAsBase64 = documentContentAsBase64;
            this.DocumentPreviewAsBase64 = documentPreviewAsBase64;
            this.IsSoftDeleted = isSoftDeleted;
            this.DeleteIsNotAllowedBefore = deleteIsNotAllowedBefore;
            this.MustBeHardDeletedAfter = mustBeHardDeletedAfter;
            this.GroupOfBusinessOwner = groupOfBusinessOwner;
            this.AssignedLanguages = assignedLanguages;
            this.AddedByUserId = addedByUserId;
        }
    }
}
