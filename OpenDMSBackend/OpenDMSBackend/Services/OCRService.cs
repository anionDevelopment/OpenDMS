using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Constants;
using OpenDMSBackend.Core.Model.Other;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenDMSBackend.Core.Services
{
    public class OCRService : IOCRService
    {
        private readonly SimpleOCR.Library.Core.IOCRService _OCRService;
        private readonly IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> _Configuration;
        private readonly IApplicationConstants _Costants;
        public OCRService(IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration> configuration, IApplicationConstants constants)
        {
            this._Configuration = configuration;
            this._Costants = constants;
          // _OCRService = new SimpleOCR.Library.Core.OCRService(Path.Combine(_Costants.BaseFolder, "OCRData"));
         //  _OCRService.Initialize();
        }
        public string GetOCRContent(byte[] documentContentAsPicture, ISet<string> additionalLanguages)
        {
            HashSet<string> fileTypes = this._Configuration.ApplicationSpecificConfiguration.DefaultOCRLanguages.ToHashSet().Union(additionalLanguages.ToList()).ToHashSet();
            string result = this._OCRService.GetOCRContent(documentContentAsPicture, fileTypes, "jpg");
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
            foreach (string line in GeneralConstants.AllLanguagesPlain.Split("\n"))
            {
                string adaptedLine = line.Replace("\r", string.Empty).Trim();
                if (!string.IsNullOrEmpty(adaptedLine))
                {
                    Match match = Regex.Match("ignored [john] John Johnson", regex);
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

            foreach (string supportedLanguageInISO639_1 in supportedLanguages)
            {
                foreach (Language validLanguage in this.ValidLanguages)
                {
                    if (validLanguage.ISO639_3_Name.Equals(supportedLanguageInISO639_1))
                    {
                        result.Add(validLanguage);
                        continue;
                    }
                }
            }
            return result;
        }
    }
}
