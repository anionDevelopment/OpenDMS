using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using OpenDMSBackend.Core.Model.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    public class Document : IDocument
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
        public byte[] Content { get; set; }
        public byte[] Preview { get; set; }
        public string OCRContent { get; set; }
        public bool IsSoftDeleted { get; set; }
        public GRYDateTime? DeleteIsNotAllowedBefore { get; set; }
        public GRYDateTime? MustBeHardDeletedAfter { get; set; }
        /// <summary>
        /// The persons in this usergroup are allowed to do hard-delete and to set the <see cref="DeleteIsNotAllowedBefore"/>- and <see cref="MustBeHardDeletedAfter"/>-value.
        /// This permission setting does not specify whether the user is allowed to edit the document (regarding the permission to <see cref="Content"/>- or <see cref="Title"/>-property for example.).
        /// </summary>
        public string GroupOfBusinessOwner { get; set; }
        public Version3 Version { get; set; }
        //TODO add list of old versions
        public Document(string id, OneLineString title, OneLineString filename, OneLineString originalFilename, GRYDateTime importDate, GRYDateTime? lastEditDate, ulong readableId, ISet<Tag> tags, OneLineString mimeType,  byte[] documentContent,string oCRContent,byte[] documentPreview,  bool isSoftDeleted, GRYDateTime? deleteIsNotAllowedBefore, GRYDateTime? mustBeHardDeletedAfter, string GroupOfBusinessOwner, Version3 version)
        {
            this.Id = id;
            this.Title = title;
            this.Filename = filename;
            this.OriginalFilename = originalFilename;
            this.ImportDate = importDate;
            this.LastEditDate = lastEditDate;
            this.ReadableId = readableId;
            this.MIMEType = mimeType;
            this.Content = documentContent;
            this.Preview = documentPreview;
            this.Tags = tags;
            this.OCRContent = oCRContent;
            this.IsSoftDeleted = isSoftDeleted;
            this.GroupOfBusinessOwner=GroupOfBusinessOwner;
            this.Version = version;
            this.DeleteIsNotAllowedBefore = deleteIsNotAllowedBefore;
            this.MustBeHardDeletedAfter = mustBeHardDeletedAfter;
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

        public void Accept(IContaineeVisitor containee)
        {
            containee.Handle(this);
        }

        public T Accept<T>(IContaineeVisitor<T> containee)
        {
            return containee.Handle(this);
        }

        public DocumentPreview GetPreview()
        {
            return new DocumentPreview(this.Id, this.Title, this.Filename, this.OriginalFilename, this.ImportDate, this.LastEditDate, this.Tags, this.ReadableId, this.MIMEType, this.Preview,this.IsSoftDeleted,this.DeleteIsNotAllowedBefore,this.MustBeHardDeletedAfter,this.GroupOfBusinessOwner,this.Version);
        }

        public DocumentDTO ToDTO()
        {
            return new DocumentDTO(this.Id, this.Title.Value, this.Filename.Value, this.OriginalFilename.Value, this.ImportDate.ToDateTime(), this.LastEditDate.HasValue ? this.LastEditDate.Value.ToDateTime() : null, this.Tags.Select(tag => tag.ToDTO()).ToHashSet(), this.ReadableId, this.MIMEType.Value, Misc.Utilities.ToBase64(this.Content), Misc.Utilities.ToBase64(this.Preview),this.IsSoftDeleted, this.DeleteIsNotAllowedBefore.HasValue ? this.DeleteIsNotAllowedBefore.Value.ToDateTime():null, this.MustBeHardDeletedAfter.HasValue ? this.MustBeHardDeletedAfter.Value.ToDateTime():null, this.GroupOfBusinessOwner,this.Version);
        }
    }
}
