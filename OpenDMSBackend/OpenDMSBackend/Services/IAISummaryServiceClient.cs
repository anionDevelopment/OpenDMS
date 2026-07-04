using OpenDMSBackend.Core.Model.BusinessTypes;

namespace OpenDMSBackend.Core.Services
{
    /// <summary>Client which generates document-summaries using an OpenAI-compatible API-endpoint.</summary>
    public interface IAISummaryServiceClient
    {
        /// <summary>Returns whether a summary-service (address) is configured and can therefore be used.</summary>
        /// <returns><see langword="true"/> if a summary-service is configured; otherwise <see langword="false"/>.</returns>
        public bool IsConfigured();

        /// <summary>Generates a short and a long summary for the given document-text.</summary>
        /// <param name="documentTitle">The title of the document (used as additional context).</param>
        /// <param name="documentText">The (OCR-)text-content of the document which should be summarized.</param>
        /// <returns>The generated <see cref="AISummary"/> containing the short and the long summary.</returns>
        public AISummary GenerateSummary(string documentTitle, string documentText);
    }
}
