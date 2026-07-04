namespace OpenDMSBackend.Core.Model.DTOs
{
    /// <summary>Data transfer object for the general (admin-configurable) OpenDMS-settings.</summary>
    public class GeneralSettingsDTO
    {
        /// <summary>Whether an AI-summary is generated automatically whenever a document is added or changed.</summary>
        public bool AutoGenerateAISummary { get; set; }

        /// <summary>Initializes a new instance of <see cref="GeneralSettingsDTO"/>.</summary>
        /// <param name="autoGenerateAISummary">Whether automatic AI-summary-generation is enabled.</param>
        public GeneralSettingsDTO(bool autoGenerateAISummary)
        {
            this.AutoGenerateAISummary = autoGenerateAISummary;
        }
    }
}
