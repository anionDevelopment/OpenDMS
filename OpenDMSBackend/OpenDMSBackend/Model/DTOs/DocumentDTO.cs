using GRYLibrary.Core.Misc.Strings;
using GRYLibrary.Core.Misc;
using System.Collections.Generic;
using System;

namespace OpenDMSBackend.Core.Model.DTOs
{
    public class DocumentDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Filename { get; set; }
        public string OriginalFilename { get; set; }
        public DateTime ImportDate { get; set; }
        public DateTime? LastEditDate { get; set; }
        public ISet<TagDTO> Tags { get; set; }
        public ulong ReadableId { get; set; }
        public byte[] DocumentContent { get; set; }
        public byte[] DocumentPreview { get; set; }

        public DocumentDTO(string id,  string title, string filename, string originalFilename, DateTime importDate, DateTime? lastEditDate, ISet<TagDTO> tags, ulong readableId, byte[] documentContent, byte[] documentPreview)
        {
            this.Id = id;
            this.Title = title;
            this.Filename = filename;
            this.OriginalFilename = originalFilename;
            this.ImportDate = importDate;
            this.LastEditDate = lastEditDate;
            this.Tags = tags;
            this.ReadableId = readableId;
            this.DocumentContent = documentContent;
            this.DocumentPreview = documentPreview;
        }
    }
}
