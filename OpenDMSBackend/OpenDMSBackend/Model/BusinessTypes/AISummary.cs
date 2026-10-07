namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    /// <summary>Holds the two AI-generated summary-texts of a document.</summary>
    public class AISummary
    {
        /// <summary>A very short summary (at most three sentences) which allows the user to quickly grasp what kind of document this is and its most concrete data.</summary>
        public string Short { get; set; }
        /// <summary>A regular summary which may be up to about one A4-page long, depending on the content and length of the document.</summary>
        public string Long { get; set; }

        /// <summary>Initializes a new instance of <see cref="AISummary"/>.</summary>
        /// <param name="shortSummary">The very short summary.</param>
        /// <param name="longSummary">The regular summary.</param>
        public AISummary(string shortSummary, string longSummary)
        {
            this.Short = shortSummary;
            this.Long = longSummary;
        }
    }
}
