using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
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
        public byte[] DocumentContent { get; set; }
        public byte[] DocumentPreview { get; set; }
        public string OCRContent { get; set; }
        public Document(string id,OneLineString title, OneLineString filename, OneLineString originalFilename, GRYDateTime importDate, GRYDateTime? lastEditDate, ulong readableId, byte[] documentContent, byte[] documentPreview, ISet<Tag> tags, string oCRContent)
        {
            this.Id = id;
            this.Title = title;
            this.Filename = filename;
            this.OriginalFilename = originalFilename;
            this.ImportDate = importDate;
            this.LastEditDate = lastEditDate;
            this.ReadableId = readableId;
            this.DocumentContent = documentContent;
            this.DocumentPreview = documentPreview;
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
            if (!this.Title.Equals(document.Title))
            {
                return false;
            }
            if (!this.Filename.Equals(document.Filename))
            {
                return false;
            }
            if (!this.OriginalFilename.Equals(document.OriginalFilename))
            {
                return false;
            }
            if (!this.ImportDate.Equals(document.ImportDate))
            {
                return false;
            }
            if (!this.LastEditDate.Equals(document.LastEditDate))
            {
                return false;
            }
            if (!this.DocumentContent.SequenceEqual(document.DocumentContent))
            {
                return false;
            }
            //document-preview will not be compared here, this is intended.
            if (!this.Tags.SetEquals(document.Tags))
            {
                return false;
            }
            //ocr-content will not be compared here, this is intended.
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
            throw new NotImplementedException();
        }
    }
}
