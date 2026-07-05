using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using OpenDMSBackend.Core.Model.DTOs;
using System;
using System.Collections.Generic;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    public class DocumentPreview
    {
        public string Id { get; set; }
        public OneLineString Title { get; set; }
        public OneLineString Filename { get; set; }
        public OneLineString OriginalFilename { get; set; }
        public DateTimeOffset ImportDate { get; set; }
        public ISet<Tag> Tags { get; set; }
        public ulong ReadableId { get; set; }
        public OneLineString MIMEType { get; set; }
        public byte[] Preview { get; set; }
        public bool IsSoftDeleted { get; set; }
        public DateTimeOffset? DeleteIsNotAllowedBefore { get; set; }
        public DateTimeOffset? MustBeHardDeletedAfter { get; set; }
        /// <summary>
        /// <see cref="Document.GroupOfBusinessOwner"/>
        /// </summary>
        public string GroupOfBusinessOwner { get; set; }
        public Version3 Version { get; set; }
        public ISet<string> AssignedLanguages { get; set; }
        public string? AddedByUserId { get; set; }
        /// <summary>
        /// A very short (at most three sentences) AI-generated summary of the document, or <see langword="null"/> if none has been generated yet.
        /// </summary>
        public string? AISummaryShort { get; set; }
        /// <summary><see cref="Document.IsLatestVersion"/></summary>
        public bool IsLatestVersion { get; set; } = true;
        /// <summary><see cref="Document.VersionNumber"/></summary>
        public int VersionNumber { get; set; } = 1;
        /// <summary><see cref="Document.VersionTimestamp"/></summary>
        public DateTimeOffset VersionTimestamp { get; set; }

        public DocumentPreview(string id, OneLineString title, OneLineString filename, OneLineString originalFilename, DateTimeOffset importDate, ISet<Tag> tags, ulong readableId, OneLineString mIMEType, byte[] preview, bool isSoftDeleted, DateTimeOffset? deleteIsNotAllowedBefore, DateTimeOffset? mustBeHardDeletedAfter, string groupOfBusinessOwner, Version3 version, ISet<string> assignedLanguages, string? addedByUserId)
        {
            this.Id = id;
            this.Title = title;
            this.Filename = filename;
            this.OriginalFilename = originalFilename;
            this.ImportDate = importDate;
            this.Tags = tags;
            this.ReadableId = readableId;
            this.MIMEType = mIMEType;
            this.Preview = preview;
            this.IsSoftDeleted = isSoftDeleted;
            this.DeleteIsNotAllowedBefore = deleteIsNotAllowedBefore;
            this.MustBeHardDeletedAfter = mustBeHardDeletedAfter;
            this.GroupOfBusinessOwner = groupOfBusinessOwner;
            this.Version = version;
            this.AssignedLanguages = assignedLanguages;
            this.AddedByUserId = addedByUserId;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is not DocumentPreview document)
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

        /// <summary>Converts this instance to its DTO representation.</summary>
        /// <returns>A <see cref="DocumentPreviewDTO"/> populated from this instance.</returns>
        public DocumentPreviewDTO ToDTO()
        {
            return new DocumentPreviewDTO(this.Id, this.Title.Value, this.Filename.Value, this.OriginalFilename.Value, GUtilities.FormatTimestamp(this.ImportDate, false), this.ReadableId, this.MIMEType.Value, Misc.Utilities.ToBase64(this.Preview), this.IsSoftDeleted, GUtilities.FormatTimestampNullable(this.DeleteIsNotAllowedBefore, false), GUtilities.FormatTimestampNullable(this.MustBeHardDeletedAfter, false), this.GroupOfBusinessOwner, this.Version, this.AssignedLanguages,this.AddedByUserId)
            {
                AISummaryShort = this.AISummaryShort,
                VersionNumber = this.VersionNumber,
                VersionTimestamp = GUtilities.FormatTimestamp(this.VersionTimestamp, false)
            };
        }

        /// <summary>Returns the most recent date of this version — the version-timestamp if set, otherwise the import date.</summary>
        /// <param name="document">The document preview to inspect.</param>
        /// <returns>The version-timestamp if set, otherwise the import date.</returns>
        public DateTimeOffset GetNewestDate(DocumentPreview document)
        {
            if (document.VersionTimestamp == default)
            {
                return document.ImportDate;
            }
            else
            {
                return document.VersionTimestamp;
            }
        }
        public override string ToString()
        {
            return $"{this.GetType().Name}[{nameof(this.ReadableId)})={this.ReadableId}, {nameof(this.Title)}=\"{this.Title.Value}\", {nameof(this.Id)}=\"{this.Id}\"]";
        }
    }
}
