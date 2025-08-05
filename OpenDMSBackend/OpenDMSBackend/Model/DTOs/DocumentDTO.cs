using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;

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
        public string MimeType { get; set; }
        public string DocumentContentAsBase64 { get; set; }
        public string DocumentPreviewAsBase64 { get; set; }
        public bool IsSoftDeleted { get; set; }
        public DateTime? DeleteIsNotAllowedBefore { get; set; }
        public DateTime? MustBeHardDeletedAfter { get; set; }
        public string GroupOfBusinessOwner { get; set; }
        public Version3 Version { get; set; }

        public DocumentDTO(string id, string title, string filename, string originalFilename, DateTime importDate, DateTime? lastEditDate, ISet<TagDTO> tags, ulong readableId, string mimeType, string documentContentAsBase64, string documentPreviewAsBase64, bool isSoftDeleted,DateTime? deleteIsNotAllowedBefore, DateTime? mustBeHardDeletedAfter, string groupOfBusinessOwner, Version3 version)
        {
            this.Id = id;
            this.Title = title;
            this.Filename = filename;
            this.OriginalFilename = originalFilename;
            this.ImportDate = importDate;
            this.LastEditDate = lastEditDate;
            this.Tags = tags;
            this.ReadableId = readableId;
            this.MimeType = mimeType;
            this.DocumentContentAsBase64 = documentContentAsBase64;
            this.DocumentPreviewAsBase64 = documentPreviewAsBase64;
            this.IsSoftDeleted = isSoftDeleted;
            this.DeleteIsNotAllowedBefore = deleteIsNotAllowedBefore;
            this.MustBeHardDeletedAfter = mustBeHardDeletedAfter;
            this.GroupOfBusinessOwner = groupOfBusinessOwner;
            this.Version = version;
        }
    }
}
