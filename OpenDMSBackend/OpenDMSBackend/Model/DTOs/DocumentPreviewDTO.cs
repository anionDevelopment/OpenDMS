using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using System;

namespace OpenDMSBackend.Core.Model.DTOs
{
    public class DocumentPreviewDTO
    {
        public string Id { get; set; }
        public OneLineString Title { get; set; }
        public OneLineString Filename { get; set; }
        public OneLineString OriginalFilename { get; set; }
        public GRYDateTime ImportDate { get; set; }
        public GRYDateTime? LastEditDate { get; set; }
        public ulong ReadableId { get; set; }
        public byte[] Preview { get; set; }

        public DocumentPreviewDTO(string id,  OneLineString title, OneLineString filename, OneLineString originalFilename, GRYDateTime importDate, GRYDateTime? lastEditDate, ulong readableId, byte[] preview)
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
