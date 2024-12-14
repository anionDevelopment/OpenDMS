using GRYLibrary.Core.Misc.Strings;
using System;

namespace OpenDMSBackend.Core.Model.DTOs
{
    public class DocumentPreviewDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Filename { get; set; }
        public string OriginalFilename { get; set; }
        public DateTime ImportDate { get; set; }
        public DateTime? LastEditDate { get; set; }
        public ulong ReadableId { get; set; }
        public byte[] Preview { get; set; }

        public DocumentPreviewDTO(string id, string title, string filename, string originalFilename, DateTime importDate, DateTime? lastEditDate, ulong readableId, byte[] preview)
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
    }
}
