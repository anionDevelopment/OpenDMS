using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using OpenDMSBackend.Core.Model.DTOs;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    public class DocumentPreview
    {
        public string Id { get; set; }
        public OneLineString Title { get; set; }
        public OneLineString Filename { get; set; }
        public OneLineString OriginalFilename { get; set; }
        public GRYDateTime ImportDate { get; set; }
        public GRYDateTime? LastEditDate { get; set; }
        public ISet<Tag> Tags { get; set; }
        public ulong ReadableId { get; set; }
        public OneLineString MIMEType { get; set; }
        public byte[] Preview { get; set; }
        public bool IsSoftDeleted { get; set; }
        public GRYDateTime? DeleteIsNotAllowedBefore { get; set; }
        public GRYDateTime? MustBeHardDeletedAfter { get; set; }
        /// <summary>
        /// <see cref="Document.GroupOfBusinessOwner"/>
        /// </summary>
        public string GroupOfBusinessOwner { get; set; }
        public Version3 Version { get; set; }

        public DocumentPreview(string id, OneLineString title, OneLineString filename, OneLineString originalFilename, GRYDateTime importDate, GRYDateTime? lastEditDate, ISet<Tag> tags, ulong readableId, OneLineString mIMEType, byte[] preview, bool isSoftDeleted, GRYDateTime? deleteIsNotAllowedBefore, GRYDateTime? mustBeHardDeletedAfter, string groupOfBusinessOwner, Version3 version)
        {
            this.Id = id;
            this.Title = title;
            this.Filename = filename;
            this.OriginalFilename = originalFilename;
            this.ImportDate = importDate;
            this.LastEditDate = lastEditDate;
            this.Tags = tags;
            this.ReadableId = readableId;
            this.MIMEType = mIMEType;
            this.Preview = preview;
            this.IsSoftDeleted= isSoftDeleted;
            this.DeleteIsNotAllowedBefore = deleteIsNotAllowedBefore;
            this.MustBeHardDeletedAfter = mustBeHardDeletedAfter;
            this.GroupOfBusinessOwner = groupOfBusinessOwner;
            this.Version = version;
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

        public DocumentPreviewDTO ToDTO()
        {
            return new DocumentPreviewDTO(this.Id, this.Title.Value, this.Filename.Value, this.OriginalFilename.Value, this.ImportDate.ToDateTime(), this.LastEditDate.HasValue ? this.LastEditDate.Value.ToDateTime() : null, this.ReadableId, this.MIMEType.Value, Misc.Utilities.ToBase64(this.Preview),this.IsSoftDeleted,this.DeleteIsNotAllowedBefore,this.MustBeHardDeletedAfter,this.GroupOfBusinessOwner,this.Version);
        }

        public GRYDateTime GetNewestDate(DocumentPreview document)
        {
            if (document.LastEditDate == default)
            {
                return document.ImportDate;
            }
            else
            {
                return document.LastEditDate!.Value;
            }
        }
    }
}
