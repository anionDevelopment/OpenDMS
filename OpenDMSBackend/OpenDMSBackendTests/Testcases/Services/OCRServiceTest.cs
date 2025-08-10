using GRYLibrary.Core.APIServer.Settings;
using GRYLibrary.Core.APIServer.Settings.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenDMSBackend.Tests.Testcases.Services
{
    [TestClass]
    public class OCRServiceTest
    {

        [TestMethod]
        public void LanguagesAreLoadableTest()
        {
            // arrange
            Mock<IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>> configurationMock = new Mock<IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>>(MockBehavior.Strict);
            Mock<CodeUnitSpecificConfiguration> codeunitMock = new Mock<CodeUnitSpecificConfiguration>();
            codeunitMock.SetupGet(mock => mock.DefaultOCRLanguages).Returns(new HashSet<string>());
            configurationMock.SetupGet(mock => mock.ApplicationSpecificConfiguration).Returns(codeunitMock.Object);
            OCRService ocrService = new OCRService(configurationMock.Object, GetTessDataFolder());
            ocrService.Initialize();

            // act
            var supportedLanguages = ocrService.SupportedLanguages;

            // assert
            Assert.IsLessThan(supportedLanguages.Count, 10);
            Assert.IsTrue(supportedLanguages.Where(language => language.ISO639_1_Name == "en").Any());
            Assert.IsTrue(supportedLanguages.Where(language => language.ISO639_1_Name == "de").Any());
            Assert.IsTrue(supportedLanguages.Where(language => language.ISO639_1_Name == "fr").Any());
            Assert.IsTrue(supportedLanguages.Where(language => language.ISO639_1_Name == "it").Any());
            Assert.IsTrue(supportedLanguages.Where(language => language.ISO639_1_Name == "es").Any());
            Assert.IsTrue(supportedLanguages.Where(language => language.ISO639_1_Name == "pt").Any());
            Assert.IsTrue(supportedLanguages.Where(language => language.ISO639_1_Name == "ja").Any());
            Assert.IsTrue(supportedLanguages.Where(language => language.ISO639_1_Name == "ru").Any());
            Assert.IsTrue(supportedLanguages.Where(language => language.ISO639_1_Name == "ar").Any());
            Assert.IsTrue(supportedLanguages.Where(language => language.ISO639_1_Name == "hi").Any());
            Assert.IsTrue(supportedLanguages.Where(language => language.ISO639_1_Name == "vi").Any());
            Assert.IsTrue(supportedLanguages.Where(language => language.ISO639_1_Name == "th").Any());
        }

        [TestMethod]
        public void WriteSupportedLanguagesToFile()
        {
            Mock<IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>> configurationMock = new Mock<IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>>(MockBehavior.Strict);
            Mock<CodeUnitSpecificConfiguration> codeunitMock = new Mock<CodeUnitSpecificConfiguration>();
            codeunitMock.SetupGet(mock => mock.DefaultOCRLanguages).Returns(new HashSet<string>());
            OCRService ocrService = new OCRService(configurationMock.Object, GetTessDataFolder());
            ocrService.Initialize();

            var targetFolderResources = Path.Combine(GeneralConstants.CodeUnitFolder, "Other", "Resources", "SupportedLanguages");
            GRYLibrary.Core.Misc.Utilities.EnsureDirectoryDoesNotExist(targetFolderResources);
            GRYLibrary.Core.Misc.Utilities.EnsureDirectoryExists(targetFolderResources);
            var targetFileResourecs = Path.Combine(targetFolderResources, "SupportedLanguages.txt");
            GRYLibrary.Core.Misc.Utilities.EnsureFileExists(targetFileResourecs);
            File.WriteAllText(targetFileResourecs, string.Join("\n", ocrService.SupportedLanguages.Select(language => $"{language.Name};{language.ISO639_1_Name};{language.ISO639_3_Name}")));

            var targetFileTable = Path.Combine(targetFolderResources, "Other", "Reference", "ReferenceContent", "images", "SupportedLanguages.plantuml");
            GRYLibrary.Core.Misc.Utilities.EnsureFileExists(targetFileTable);
            string contentTable = @"@startuml
map SupportedLanguages {
Name => ISO639-1";
            foreach (var language in ocrService.SupportedLanguages)
            {
                contentTable = contentTable + $"\n  {language.Name} => {language.ISO639_1_Name}";
            }
            contentTable = contentTable + @"
}
@enduml
";
            File.WriteAllText(targetFileTable, contentTable);

        }
        private IApplicationConstants GetTessDataFolder()
        {
            Mock<IApplicationConstants> configurationMock = new Mock<IApplicationConstants>(MockBehavior.Strict);
            configurationMock.SetupGet(mock => mock.GetDataFolder()).Returns(Path.Combine(GeneralConstants.CodeUnitFolder, "Other", "Resources", "OCRData"));
            return configurationMock.Object;
        }
    }
}
