using GRYLibrary.Core.APIServer.Services;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;
using Namotion.Reflection;
using OpenDMSBackend.Core.Configuration;
using SimpleOCR.Library.Core.FileTypes;
using SimpleOCR.Library.Core.Other;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace OpenDMSBackend.Core.Services
{
    public sealed class OCRServiceClient : IOCRServiceClient, IExternalService
    {
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _Configuration;
        private readonly IGRYLog _Log;

        public bool IsInitialized => throw new NotImplementedException();

        public OCRServiceClient(IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration, IGRYLog log, CommandlineParameter cmdParameter)
        {
            this._Configuration = configuration;
            this._Log = log;
        }
        public bool IsAvailable()
        {
            try
            {
                this.GetSupportedLanguages();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }
        private HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            if (!String.IsNullOrWhiteSpace(this._Configuration.ApplicationSpecificConfiguration.OCRDataServiceAPIKey))
            {
                client.DefaultRequestHeaders.Add("X-ApiKey", this._Configuration.ApplicationSpecificConfiguration.OCRDataServiceAPIKey);
            }
            return client;
        }

        public string GetOCRContent(byte[] fileContent, FileType fileType, ISet<string> languages)
        {
            using HttpClient httpClient = this.GetHttpClient();
            using MultipartFormDataContent content = new MultipartFormDataContent();

            ByteArrayContent fileContent2 = new ByteArrayContent(fileContent);
            fileContent2.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent2, "name", "fileName");
            HttpResponseMessage response = httpClient.PutAsync(this.GetAPIBasePath() + $"/GetOCRContent?fileType={Uri.EscapeDataString(FileType.Serialize(fileType))}&mimeType={languages}", content).WaitAndGetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().WaitAndGetResult();
        }

        public byte[] ToPicture(byte[] fileContent, FileType fileType, string mimeType)
        {
            using HttpClient httpClient = this.GetHttpClient();
            using MultipartFormDataContent content = new MultipartFormDataContent();

            ByteArrayContent fileContent2 = new ByteArrayContent(fileContent);
            fileContent2.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent2, "name", "fileName");
            HttpResponseMessage response = httpClient.PutAsync(this.GetAPIBasePath() + $"/ToPicture?fileType={Uri.EscapeDataString(FileType.Serialize(fileType))}&mimeType={mimeType}", content).WaitAndGetResult();
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsByteArrayAsync().WaitAndGetResult();
        }

        private string GetAPIBasePath()
        {
            return this._Configuration.ApplicationSpecificConfiguration.OCRDataServiceAddress + "/API/V1/SimpleOCR";
        }

        public ISet<Language> GetSupportedLanguages()
        {
            using HttpClient httpClient = this.GetHttpClient();
            HttpResponseMessage response = httpClient.GetAsync(this.GetAPIBasePath() + $"/GetSupportedLanguages").WaitAndGetResult();
            response.EnsureSuccessStatusCode();
            byte[] json = response.Content.ReadAsByteArrayAsync().WaitAndGetResult();
            ISet<Language>? result = JsonSerializer.Deserialize<ISet<Language>>(json);
            return GRYLibrary.Core.Misc.Utilities.GetValue(result);
        }

        public void ReInitialize()
        {
            throw new NotSupportedException();
        }

        public void Initialize()
        {
            throw new NotSupportedException();
        }
    }
}
