using GRYLibrary.Core.APIServer.Settings.Configuration;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackend.Core.Configuration;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities.Constants;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenDMSBackend.Tests.Testcases.Services
{
    [TestClass]
    public class OCRServiceTest
    {

        [TestMethod]
        [Ignore]
        public void LanguagesAreLoadableTest()
        {
            // arrange
            Mock<IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>> configurationMock = new Mock<IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>>(MockBehavior.Strict);
            Mock<CodeUnitSpecificConfiguration> codeunitMock = new Mock<CodeUnitSpecificConfiguration>();
            codeunitMock.SetupGet(mock => mock.DefaultOCRLanguages).Returns(new HashSet<string>());
            configurationMock.SetupGet(mock => mock.ApplicationSpecificConfiguration).Returns(codeunitMock.Object);
            OCRServiceWrapper ocrService = new OCRServiceWrapper(configurationMock.Object, GeneralLogger.CreateUsingConsole(), new CommandlineParameter()
            {
                InitialOCRDataFolder = this.GetTessDataFolder(),
            });
            ocrService.Initialize();

            // act
            ISet<Core.Model.Other.Language> supportedLanguages = ocrService.SupportedLanguages;

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
        [Ignore]
        public void WriteSupportedLanguagesToFile()
        {
            Mock<IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>> configurationMock = new Mock<IPersistedAPIServerConfiguration<CodeUnitSpecificConfiguration>>(MockBehavior.Strict);
            Mock<CodeUnitSpecificConfiguration> codeunitMock = new Mock<CodeUnitSpecificConfiguration>();
            codeunitMock.SetupGet(mock => mock.DefaultOCRLanguages).Returns(new HashSet<string>());
            OCRServiceWrapper ocrService = new OCRServiceWrapper(configurationMock.Object, GeneralLogger.CreateUsingConsole(), new CommandlineParameter()
            {
                InitialOCRDataFolder = this.GetTessDataFolder(),
            });
            ocrService.Initialize();

            string targetFolderResources = Path.Combine(GeneralConstants.CodeUnitFolder, "Other", "Resources", "SupportedLanguages");
            GRYLibrary.Core.Misc.Utilities.EnsureDirectoryDoesNotExist(targetFolderResources);
            GRYLibrary.Core.Misc.Utilities.EnsureDirectoryExists(targetFolderResources);
            string targetFileResourecs = Path.Combine(targetFolderResources, "SupportedLanguages.txt");
            GRYLibrary.Core.Misc.Utilities.EnsureFileExists(targetFileResourecs);
            IOrderedEnumerable<Core.Model.Other.Language> supportedLanguagesOrdered = ocrService.SupportedLanguages.OrderBy(language => language.Name);
            File.WriteAllText(targetFileResourecs, string.Join("\n", supportedLanguagesOrdered.Select(language => $"{language.Name};{language.ISO639_1_Name};{language.ISO639_3_Name}")));

            string targetFolder = Path.Combine(targetFolderResources, "Other", "Reference", "ReferenceContent", "images");
            GRYLibrary.Core.Misc.Utilities.EnsureDirectoryExists(targetFolder);
            string targetFileTable = Path.Combine(targetFolder, "SupportedLanguages.plantuml");
            GRYLibrary.Core.Misc.Utilities.EnsureFileExists(targetFileTable);
            string contentTable = @"@startuml Supported languages
'This file is generated.
map SupportedLanguages {
Name => ISO639-1";
            foreach (Core.Model.Other.Language language in supportedLanguagesOrdered)
            {
                contentTable = contentTable + $"\n  {language.Name} => {language.ISO639_1_Name}";
            }
            contentTable = contentTable + @"
}
@enduml
";
            File.WriteAllText(targetFileTable, contentTable);

        }
        private string GetTessDataFolder()
        {
            return Path.Combine(GeneralConstants.CodeUnitFolder, "Other", "Resources", "OCRData");
        }
    }
}
