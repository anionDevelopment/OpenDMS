using GRYLibrary.Core.Misc;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.DTOs
{
    public class DocumentPreviewDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Filename { get; set; }
        public string OriginalFilename { get; set; }
        public string ImportDate { get; set; }
        public string? LastEditDate { get; set; }
        public ulong ReadableId { get; set; }
        public string MimeType { get; set; }
        public string PreviewAsBase64 { get; set; }
        public bool IsSoftDeleted { get; set; }
        public string? DeleteIsNotAllowedBefore { get; set; }
        public string? MustBeHardDeletedAfter { get; set; }
        public string GroupOfBusinessOwner { get; set; }
        public Version3 Version { get; set; }
        public ISet<string> AssignedLanguages { get; set; }

        public DocumentPreviewDTO(string id, string title, string filename, string originalFilename, string importDate, string? lastEditDate, ulong readableId, string mimeType, string previewAsBase64, bool isSoftDeleted, string? deleteIsNotAllowedBefore, string? mustBeHardDeletedAfter, string groupOfBusinessOwner, Version3 version, ISet<string> assignedLanguages)
        {
            this.Id = id;
            this.Title = title;
            this.Filename = filename;
            this.OriginalFilename = originalFilename;
            this.ImportDate = importDate;
            this.LastEditDate = lastEditDate;
            this.ReadableId = readableId;
            this.MimeType = mimeType;
            this.PreviewAsBase64 = previewAsBase64;
            this.IsSoftDeleted = isSoftDeleted;
            this.DeleteIsNotAllowedBefore = deleteIsNotAllowedBefore;
            this.MustBeHardDeletedAfter = mustBeHardDeletedAfter;
            this.GroupOfBusinessOwner = groupOfBusinessOwner;
            this.Version = version;
            this.AssignedLanguages = assignedLanguages;
        }
    }
}
