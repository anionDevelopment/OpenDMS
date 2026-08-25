using GRYLibrary.Core.APIServer.Services;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.APIServer.Utilities.InitializationStates;
using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Configuration;
using SimpleOCR.Library.Core.Other;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Services
{
    public sealed class OCRServiceClientMock : IOCRServiceClient, IExternalService
    {
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _Configuration;
        private readonly IGRYLog _Log;

        public InitializationState InitializationState { get; private set; } = new Initialized();

        /// <summary>Initializes a new instance of <see cref="OCRServiceClientMock"/> that returns empty results for all OCR calls.</summary>
        /// <param name="log">The logger for diagnostic output.</param>
        /// <param name="cmdParameter">Commandline parameters (reserved for future use).</param>
        /// <param name="configuration">The persisted server configuration.</param>
        public OCRServiceClientMock(IServerLog log, CommandlineParameter cmdParameter, IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration)
        {
            this._Configuration = configuration;
            this._Log = log.Logger;
        }
        /// <inheritdoc />
        public (bool, Exception?) IsAvailable()
        {
            return (true, null);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }

        /// <inheritdoc />
        public string GetOCRContent(byte[] fileContent, string mimeType, ISet<string> languages)
        {
            return string.Empty;
        }

        /// <inheritdoc />
        public byte[] ToPicture(byte[] fileContent, string mimeType)
        {
            //the mock returns empty results for all OCR-calls.
            return Array.Empty<byte>();
        }

        /// <inheritdoc />
        public ISet<Language> GetSupportedLanguages()
        {
            return new HashSet<Language>();
        }

        /// <inheritdoc />
        public void ReInitialize()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }

        /// <inheritdoc />
        public void Initialize()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }

        /// <inheritdoc />
        public void WaitUntilAvailable(TimeSpan timeSpan)
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }
    }
}
