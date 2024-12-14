using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using System;
using System.Linq;

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
        public ulong ReadableId { get; set; }
        public byte[] Preview { get; set; }
        public DocumentPreview(string id, OneLineString title, OneLineString filename, OneLineString originalFilename, GRYDateTime importDate, GRYDateTime? lastEditDate, ulong readableId, byte[] preview)
        {
            this.Id = id;
            this.Title = title;
            this.Filename = filename;
            this.OriginalFilename = originalFilename;
            this.ImportDate = importDate;
            this.LastEditDate = lastEditDate;
            this.ReadableId = readableId;
            this.Preview = preview;
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
            if (!this.Preview.SequenceEqual(document.Preview))
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Id);
        }

    }
}
