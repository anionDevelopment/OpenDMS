using GRYLibrary.Core.APIServer.Services;
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

        public OCRServiceClientMock(IGRYLog log, CommandlineParameter cmdParameter, IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration)
        {
            this._Configuration = configuration;
            this._Log = log;
        }
        public (bool, Exception?) IsAvailable()
        {
            return (true, null);
        }

        public void Dispose()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }

        public string GetOCRContent(byte[] fileContent, string mimeType, ISet<string> languages)
        {
            return string.Empty;
        }

        public byte[] ToPicture(byte[] fileContent, string mimeType)
        {
            throw new NotImplementedException();
        }

        public ISet<Language> GetSupportedLanguages()
        {
            return new HashSet<Language>();
        }

        public void ReInitialize()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }

        public void Initialize()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }

        public void WaitUntilAvailable(TimeSpan timeSpan)
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }
    }
}
