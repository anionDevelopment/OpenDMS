namespace OpenDMSBackend.Core.Model.DTOs
{
    /// <summary>Data transfer object for the retention-dates of a document.</summary>
    public class RetentionDatesDTO
    {
        /// <summary>The point in time before which the document must not be hard-deleted, as iso-8601-timestamp (for example "2026-01-31T12:00:00+01:00"), or <see langword="null"/> when the document has no deletion-lock.</summary>
        public string? DeleteIsNotAllowedBefore { get; set; }
        /// <summary>The point in time after which the document must be hard-deleted by the scheduled housekeeping, as iso-8601-timestamp, or <see langword="null"/> when the document must not be deleted automatically.</summary>
        public string? MustBeHardDeletedAfter { get; set; }

        /// <summary>Initializes a new instance of <see cref="RetentionDatesDTO"/>.</summary>
        /// <param name="deleteIsNotAllowedBefore">The point in time before which the document must not be hard-deleted, or <see langword="null"/>.</param>
        /// <param name="mustBeHardDeletedAfter">The point in time after which the document must be hard-deleted, or <see langword="null"/>.</param>
        public RetentionDatesDTO(string? deleteIsNotAllowedBefore, string? mustBeHardDeletedAfter)
        {
            this.DeleteIsNotAllowedBefore = deleteIsNotAllowedBefore;
            this.MustBeHardDeletedAfter = mustBeHardDeletedAfter;
        }
    }
}
