using OpenDMSBackend.Core.Model.BusinessTypes;

namespace OpenDMSBackend.Core.Services
{
    /// <summary>A mock-implementation of <see cref="IAISummaryServiceClient"/> which returns deterministic summaries without calling any external service. Used in development and in tests.</summary>
    public sealed class AISummaryServiceClientMock : IAISummaryServiceClient
    {
        /// <inheritdoc />
        public bool IsConfigured()
        {
            return true;
        }

        /// <inheritdoc />
        public AISummary GenerateSummary(string documentTitle, string documentText)
        {
            string shortSummary = $"Mock-short-summary of document '{documentTitle}'.";
            string longSummary = $"Mock-long-summary of document '{documentTitle}'. The document contains {documentText.Length} characters of text-content.";
            return new AISummary(shortSummary, longSummary);
        }
    }
}
