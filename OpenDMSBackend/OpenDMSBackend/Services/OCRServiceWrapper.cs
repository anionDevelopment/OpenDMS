using GRYLibrary.Core.APIServer.Services;
using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Exceptions;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenDMSBackend.Core.Services
{
    public class OCRServiceWrapper : IOCRServiceWrapper, IExternalService
    {
        private bool _IsAvailable = false;
        private readonly SimpleOCR.Library.Core.IOCRService _OCRService;
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _Configuration;
        private readonly IApplicationConstants _Costants;
        private readonly CommandlineParameter _CMDParameter;
        private readonly string OCRDataFolder;
        public OCRServiceWrapper(IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration, IApplicationConstants constants, IGRYLog log, CommandlineParameter cmdParameter)
        {
            try
            {
                this._Configuration = configuration;
                this._Costants = constants;
                string ocrDataFolder = GRYLibrary.Core.Misc.Utilities.AssertNotNull<string>(cmdParameter.OCRDataFolder, nameof(cmdParameter.OCRDataFolder));
                ;
                if (!GRYLibrary.Core.Misc.Utilities.IsAbsoluteLocalFilePath(ocrDataFolder))
                {
                    ocrDataFolder = GRYLibrary.Core.Misc.Utilities.ResolveToFullPath(ocrDataFolder);
                }
                this.OCRDataFolder = ocrDataFolder;
                this._OCRService = new SimpleOCR.Library.Core.OCRService(this.OCRDataFolder, log);
            }
            catch (Exception e)
            {
                log.LogException(e, "OCR-service not available.", Microsoft.Extensions.Logging.LogLevel.Warning);
            }
        }
        public string GetOCRContent(byte[] documentContentAsPicture, ISet<string> additionalLanguages)
        {
            if (!this.IsAvailable())
            {
                throw new ServiceNotAvailableException();
            }
            HashSet<string> languages = this._Configuration.ApplicationSpecificConfiguration.DefaultOCRLanguages.ToHashSet().Union(additionalLanguages.ToList()).ToHashSet();
            string result = this._OCRService.GetOCRContent(documentContentAsPicture, languages);
            return result;
        }

        private ISet<Language>? _ValidLanguages = null;
        private ISet<Language> ValidLanguages
        {
            get
            {
                if (this._ValidLanguages == null)
                {
                    this._ValidLanguages = this.GetValidLanguages();
                }
                return this._ValidLanguages;
            }
        }
        private ISet<Language> GetValidLanguages()
        {
            HashSet<Language> result = new HashSet<Language>();
            string regex = @"(.+) \(([a-z][a-z])\;\ ([a-z][a-z][a-z])\)"; //adaptedLine is like "Norwegian Bokmål (nb; nob)"
            string[] lines = GeneralConstants.AllLanguagesPlain.Split("\n");
            foreach (string line in lines)
            {
                string adaptedLine = line.Replace("\r", string.Empty).Trim();
                if (!string.IsNullOrEmpty(adaptedLine))
                {
                    Match match = Regex.Match(adaptedLine, regex);
                    if (match.Success)
                    {
                        result.Add(new Language(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value));
                    }
                }
            }
            return result;
        }

        private ISet<Language>? _SupportedLanguages = null;
        public ISet<Language> SupportedLanguages
        {
            get
            {
                if (!this.IsAvailable())
                {
                    throw new ServiceNotAvailableException();
                }
                if (this._SupportedLanguages == null)
                {
                    this._SupportedLanguages = this.GetSupportedLanguages();
                }
                return this._SupportedLanguages;
            }
        }
        private ISet<Language> GetSupportedLanguages()
        {
            HashSet<Language> result = new HashSet<Language>();
            IEnumerable<string> supportedLanguages = this._OCRService.GetSupportedLanguages();

            foreach (string supportedLanguageInISO639_3 in supportedLanguages)
            {
                foreach (Language validLanguage in this.ValidLanguages)
                {
                    if (validLanguage.ISO639_3_Name.Equals(supportedLanguageInISO639_3))
                    {
                        result.Add(validLanguage);
                        continue;
                    }
                }
            }
            return result;
        }

        public void Initialize()
        {
            try
            {
                if (this.OCRDataFolder != null)
                {
                    GRYLibrary.Core.Misc.Utilities.AssertNotNull(this.OCRDataFolder, nameof(this.OCRDataFolder));
                    this._OCRService.Initialize();
                    this._IsAvailable = true;
                }
            }
            catch
            {
                GRYLibrary.Core.Misc.Utilities.NoOperation();
            }
        }

        public bool IsAvailable()
        {
            return this._IsAvailable;
        }

        public void Dispose()
        {
            GRYLibrary.Core.Misc.Utilities.NoOperation();
        }
    }
}
