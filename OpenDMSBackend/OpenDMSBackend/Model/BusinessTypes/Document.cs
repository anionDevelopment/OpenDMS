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
        public Document(string id, OneLineString title, OneLineString filename, OneLineString originalFilename, GRYDateTime importDate, GRYDateTime? lastEditDate, ulong readableId, OneLineString mimeType, byte[] documentContent, byte[] documentPreview, ISet<Tag> tags, string oCRContent)
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
            return new DocumentPreview(this.Id, this.Title, this.Filename, this.OriginalFilename, this.ImportDate, this.LastEditDate, this.ReadableId, this.Tags, this.MIMEType, this.Preview);
        }

        public DocumentDTO ToDTO()
        {
            return new DocumentDTO(this.Id, this.Title.Value, this.Filename.Value, this.OriginalFilename.Value, this.ImportDate.ToDateTime(), this.LastEditDate.HasValue ? this.LastEditDate.Value.ToDateTime() : null, this.Tags.Select(tag => tag.ToDTO()).ToHashSet(), this.ReadableId, this.MIMEType.Value, Miscellaneous.Utilities.ToBase64(this.Content), Miscellaneous.Utilities.ToBase64(this.Preview));
        }
    }
}
