using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Model.BusinessTypes;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace OpenDMSBackend.Core.Services
{
    /// <summary>Generates document-summaries by calling an OpenAI-compatible chat-completions-endpoint (for example OpenAI itself or a compatible self-hosted service).</summary>
    public sealed class AISummaryServiceClient : IAISummaryServiceClient
    {
        private const string DefaultModel = "gpt-4o-mini";
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _Configuration;
        private readonly IGRYLog _Log;

        /// <summary>Initializes a new instance of <see cref="AISummaryServiceClient"/>.</summary>
        /// <param name="configuration">The persisted server-configuration, including the summary-service address, API-key and model.</param>
        /// <param name="log">The logger for diagnostic output.</param>
        public AISummaryServiceClient(IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration, IServerLog log)
        {
            this._Configuration = configuration;
            this._Log = log.Logger;
        }

        /// <inheritdoc />
        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(this._Configuration.ApplicationSpecificConfiguration.AISummaryServiceAddress);
        }

        /// <inheritdoc />
        public AISummary GenerateSummary(string documentTitle, string documentText)
        {
            if (!this.IsConfigured())
            {
                throw new InvalidOperationException("No AI-summary-service is configured (AISummaryServiceAddress is not set).");
            }
            string responseContent = this.RequestChatCompletion(documentTitle, documentText);
            return this.ParseSummary(responseContent);
        }

        private string RequestChatCompletion(string documentTitle, string documentText)
        {
            CodeUnitSpecificConfiguration configuration = this._Configuration.ApplicationSpecificConfiguration;
            string model = string.IsNullOrWhiteSpace(configuration.AISummaryServiceModel) ? DefaultModel : configuration.AISummaryServiceModel!;
            string systemPrompt =
                "You are a document-summarization-assistant for a document-management-system. "
                + "You always answer with a single JSON-object with exactly the two string-properties \"shortSummary\" and \"longSummary\". "
                + "\"shortSummary\": at most three sentences which allow a user to quickly grasp what kind of document this is and its most concrete data (for example sender, customer-number or amounts) if available. "
                + "\"longSummary\": a regular summary of the document, up to about one A4-page long depending on the content and length of the document. "
                + "Answer in the same language as the document. Do not add any text outside of the JSON-object.";
            string userPrompt = $"Title: {documentTitle}\n\nContent:\n{documentText}";

            object requestBody = new
            {
                model,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt },
                },
                response_format = new { type = "json_object" },
            };
            string requestJson = JsonSerializer.Serialize(requestBody);

            using HttpClient httpClient = this.GetHttpClient();
            using StringContent content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            HttpResponseMessage response = httpClient.PostAsync(this.GetChatCompletionsUrl(), content).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();
            string responseJson = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return this.ExtractMessageContent(responseJson);
        }

        private string ExtractMessageContent(string responseJson)
        {
            using JsonDocument document = JsonDocument.Parse(responseJson);
            return document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }

        private AISummary ParseSummary(string messageContent)
        {
            try
            {
                using JsonDocument document = JsonDocument.Parse(messageContent);
                string shortSummary = document.RootElement.TryGetProperty("shortSummary", out JsonElement shortElement) ? (shortElement.GetString() ?? string.Empty) : string.Empty;
                string longSummary = document.RootElement.TryGetProperty("longSummary", out JsonElement longElement) ? (longElement.GetString() ?? string.Empty) : string.Empty;
                return new AISummary(shortSummary, longSummary);
            }
            catch (JsonException exception)
            {
                //if the model did not return valid JSON the whole answer is used as the long-summary so that no information is lost.
                this._Log.Log("The AI-summary-service did not return a valid JSON-object; using the raw answer as long-summary.", exception);
                return new AISummary(string.Empty, messageContent);
            }
        }

        private HttpClient GetHttpClient()
        {
            HttpClient client = new HttpClient();
            string? apiKey = this._Configuration.ApplicationSpecificConfiguration.AISummaryServiceAPIKey;
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            }
            return client;
        }

        private string GetChatCompletionsUrl()
        {
            string address = this._Configuration.ApplicationSpecificConfiguration.AISummaryServiceAddress!.TrimEnd('/');
            //allow the admin to configure either the base-address or the full chat-completions-address.
            if (address.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
            {
                return address;
            }
            if (address.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
            {
                return address + "/chat/completions";
            }
            return address + "/v1/chat/completions";
        }
    }
}
