using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using OpenDMSBackend.Core.Model.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    public class Document : IDocument
    {
        public string Id { get; set; }
        public OneLineString Title { get; set; }
        public OneLineString Filename { get; set; }
        public OneLineString OriginalFilename { get; set; }
        public DateTimeOffset ImportDate { get; set; }
        public ISet<Tag> Tags { get; set; }
        /// <summary>
        /// The values this document holds for the custom metadata-fields defined at its containing storage-location, keyed by the field-definition-id (see <see cref="MetadataFieldDefinition"/>).
        /// A boolean-value is stored as its lower-case string-representation ("true"/"false"). A field for which the document has no value is simply absent from this dictionary.
        /// </summary>
        public IDictionary<string, string> MetadataValues { get; set; } = new Dictionary<string, string>();
        public ulong ReadableId { get; set; }
        public OneLineString MIMEType { get; set; }
        public byte[] Content { get; set; }
        public byte[] Preview { get; set; }
        public string OCRContent { get; set; }
        public bool IsSoftDeleted { get; set; }
        /// <summary>
        /// Whether this document has been hard-deleted (regulated deletion): its binary-content and preview are removed, its <see cref="OCRContent"/> and AI-summaries are cleared and its tags are unassigned, but the row itself is kept for traceability. Hard-deleted documents are excluded from the normal listings (together with the <see cref="IsLatestVersion"/>-filter).
        /// </summary>
        public bool IsHardDeleted { get; set; } = false;
        public DateTimeOffset? DeleteIsNotAllowedBefore { get; set; }
        public DateTimeOffset? MustBeHardDeletedAfter { get; set; }
        /// <summary>
        /// The persons in this usergroup are allowed to do hard-delete and to set the <see cref="DeleteIsNotAllowedBefore"/>- and <see cref="MustBeHardDeletedAfter"/>-value.
        /// This permission setting does not specify whether the user is allowed to edit the document (regarding the permission to <see cref="Content"/>- or <see cref="Title"/>-property for example.).
        /// </summary>
        public string GroupOfBusinessOwner { get; set; }
        public ISet<string> AssignedLanguages { get; set; }
        public string? AddedByUserId { get; set; }
        /// <summary>
        /// A very short (at most three sentences) AI-generated summary of the document which allows the user to quickly grasp what kind of document this is and its most concrete data (for example sender or customer-number).
        /// <see langword="null"/> if no AI-summary has been generated yet.
        /// </summary>
        public string? AISummaryShort { get; set; }
        /// <summary>
        /// A regular (potentially up to about one A4-page long) AI-generated summary of the document.
        /// <see langword="null"/> if no AI-summary has been generated yet.
        /// </summary>
        public string? AISummaryLong { get; set; }
        /// <summary>
        /// Whether this document-row is the latest version of its logical document. Queries which mean "the current document" (loading, search, latest-documents) only consider rows where this is <see langword="true"/>.
        /// </summary>
        public bool IsLatestVersion { get; set; } = true;
        /// <summary>
        /// The (1-based, incrementing) version-number of this document within its version-chain. Populated from the <c>DocumentVersion</c>-table when the document is loaded.
        /// </summary>
        public int VersionNumber { get; set; } = 1;
        /// <summary>
        /// The moment this version was created. Populated from the <c>DocumentVersion</c>-table when the document is loaded.
        /// </summary>
        public DateTimeOffset VersionTimestamp { get; set; }
        /// <summary>
        /// Initializes a new instance of <see cref="Document"/>.
        /// </summary>
        /// <param name="id">The unique identifier of the document.</param>
        /// <param name="title">The title of the document.</param>
        /// <param name="filename">The stored filename of the document.</param>
        /// <param name="originalFilename">The original filename as uploaded.</param>
        /// <param name="importDate">The date and time the document was imported.</param>
        /// <param name="readableId">The human-readable numeric identifier.</param>
        /// <param name="tags">The set of tags assigned to the document.</param>
        /// <param name="mimeType">The MIME type of the document content.</param>
        /// <param name="documentContent">The raw binary content of the document.</param>
        /// <param name="oCRContent">The OCR-extracted text content of the document.</param>
        /// <param name="documentPreview">The binary preview image of the document.</param>
        /// <param name="isSoftDeleted">Whether the document is soft-deleted.</param>
        /// <param name="deleteIsNotAllowedBefore">The earliest date hard-deletion is permitted, or <see langword="null"/> if unrestricted.</param>
        /// <param name="mustBeHardDeletedAfter">The deadline for mandatory hard-deletion, or <see langword="null"/> if unrestricted.</param>
        /// <param name="GroupOfBusinessOwner">The group identifier of the business owner.</param>
        /// <param name="assignedLanguages">The set of language codes assigned to the document.</param>
        /// <param name="addedByUserId">The identifier of the user who added the document, or <see langword="null"/> if unknown.</param>
        public Document(string id, OneLineString title, OneLineString filename, OneLineString originalFilename, DateTimeOffset importDate, ulong readableId, ISet<Tag> tags, OneLineString mimeType, byte[] documentContent, string oCRContent, byte[] documentPreview, bool isSoftDeleted, DateTimeOffset? deleteIsNotAllowedBefore, DateTimeOffset? mustBeHardDeletedAfter, string GroupOfBusinessOwner, ISet<string> assignedLanguages, string? addedByUserId)
        {
            this.Id = id;
            this.Title = title;
            this.Filename = filename;
            this.OriginalFilename = originalFilename;
            this.ImportDate = importDate;
            this.ReadableId = readableId;
            this.MIMEType = mimeType;
            this.Content = documentContent;
            this.Preview = documentPreview;
            this.Tags = tags;
            this.OCRContent = oCRContent;
            this.IsSoftDeleted = isSoftDeleted;
            this.GroupOfBusinessOwner = GroupOfBusinessOwner;
            this.DeleteIsNotAllowedBefore = deleteIsNotAllowedBefore;
            this.MustBeHardDeletedAfter = mustBeHardDeletedAfter;
            this.AssignedLanguages = assignedLanguages;
            this.AddedByUserId = addedByUserId;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is not Document document)
            {
                return false;
            }
            if (!this.Id.Equals(document.Id))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Id);
        }

        /// <summary>
        /// Dispatches this instance to the appropriate <see cref="IContaineeVisitor"/> handler.
        /// </summary>
        /// <param name="containee">The visitor to dispatch to.</param>
        public void Accept(IContaineeVisitor containee)
        {
            containee.Handle(this);
        }

        /// <summary>
        /// Dispatches this instance to the appropriate <see cref="IContaineeVisitor{T}"/> handler and returns the result.
        /// </summary>
        /// <typeparam name="T">The return type of the visitor.</typeparam>
        /// <param name="containee">The visitor to dispatch to.</param>
        /// <returns>The value returned by the visitor handler.</returns>
        public T Accept<T>(IContaineeVisitor<T> containee)
        {
            return containee.Handle(this);
        }

        /// <summary>
        /// Returns a <see cref="DocumentPreview"/> representing the metadata and preview of this document.
        /// </summary>
        /// <returns>A <see cref="DocumentPreview"/> for this document.</returns>
        public DocumentPreview GetPreview()
        {
            return new DocumentPreview(this.Id, this.Title, this.Filename, this.OriginalFilename, this.ImportDate, this.Tags, this.ReadableId, this.MIMEType, this.Preview, this.IsSoftDeleted, this.DeleteIsNotAllowedBefore, this.MustBeHardDeletedAfter, this.GroupOfBusinessOwner,this.AssignedLanguages, this.AddedByUserId)
            {
                AISummaryShort = this.AISummaryShort,
                IsLatestVersion = this.IsLatestVersion,
                IsHardDeleted = this.IsHardDeleted,
                VersionNumber = this.VersionNumber,
                VersionTimestamp = this.VersionTimestamp
            };
        }

        /// <summary>
        /// Converts this instance to its DTO representation.
        /// </summary>
        /// <returns>A <see cref="DocumentDTO"/> populated from this instance.</returns>
        public DocumentDTO ToDTO()
        {
            return new DocumentDTO(this.Id, this.Title.Value, this.Filename.Value, this.OriginalFilename.Value,GUtilities.FormatTimestamp( this.ImportDate,false), this.Tags.Select(tag => tag.ToDTO()).ToHashSet(), this.ReadableId, this.MIMEType.Value, Misc.Utilities.ToBase64(this.Content), Misc.Utilities.ToBase64(this.Preview), this.IsSoftDeleted, GUtilities.FormatTimestampNullable(this.DeleteIsNotAllowedBefore,false), GUtilities.FormatTimestampNullable(this.MustBeHardDeletedAfter,false), this.GroupOfBusinessOwner, this.AssignedLanguages, this.AddedByUserId)
            {
                AISummaryShort = this.AISummaryShort,
                AISummaryLong = this.AISummaryLong,
                VersionNumber = this.VersionNumber,
                VersionTimestamp = GUtilities.FormatTimestamp(this.VersionTimestamp, false),
                MetadataValues = new Dictionary<string, string>(this.MetadataValues)
            };
        }
        public override string ToString()
        {
            return $"{this.GetType().Name}[{nameof(this.ReadableId)})={this.ReadableId}, {nameof(this.Title)}=\"{this.Title.Value}\", {nameof(this.Id)}=\"{this.Id}\"]";
        }
    }
}
