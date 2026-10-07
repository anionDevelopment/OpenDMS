using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackend.Core.Services;
using System.Collections.Generic;
using System.Linq;
using Utilities = OpenDMSBackend.Core.Misc.Utilities;

namespace OpenDMSBackend.Tests.Testcases.Miscellaneous
{
    [TestClass]
    public class UtilitiesTests
    {
        private const string ContentId = "content1";

        [TestMethod(DisplayName = nameof(DoForContentObjectInvokesTheStorageLocationActionWhenTheContentIsAStorageLocation))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void DoForContentObjectInvokesTheStorageLocationActionWhenTheContentIsAStorageLocation()
        {
            //arrange
            Mock<IPersistence> persistenceMock = new Mock<IPersistence>(MockBehavior.Strict);
            persistenceMock.Setup(p => p.IsStorageLocation(ContentId)).Returns(true);

            //act
            string actual = Utilities.DoForContentObject(persistenceMock.Object, ContentId, contentId => "storageLocation", contentId => "folder", contentId => "document");

            //assert
            Assert.AreEqual("storageLocation", actual);
            persistenceMock.Verify(p => p.IsFolder(It.IsAny<string>()), Times.Never);
            persistenceMock.Verify(p => p.IsDocument(It.IsAny<string>()), Times.Never);
        }

        [TestMethod(DisplayName = nameof(DoForContentObjectInvokesTheFolderActionWhenTheContentIsAFolder))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void DoForContentObjectInvokesTheFolderActionWhenTheContentIsAFolder()
        {
            //arrange
            Mock<IPersistence> persistenceMock = new Mock<IPersistence>(MockBehavior.Strict);
            persistenceMock.Setup(p => p.IsStorageLocation(ContentId)).Returns(false);
            persistenceMock.Setup(p => p.IsFolder(ContentId)).Returns(true);

            //act
            string actual = Utilities.DoForContentObject(persistenceMock.Object, ContentId, contentId => "storageLocation", contentId => "folder", contentId => "document");

            //assert
            Assert.AreEqual("folder", actual);
            persistenceMock.Verify(p => p.IsDocument(It.IsAny<string>()), Times.Never);
        }

        [TestMethod(DisplayName = nameof(DoForContentObjectInvokesTheDocumentActionWhenTheContentIsADocument))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void DoForContentObjectInvokesTheDocumentActionWhenTheContentIsADocument()
        {
            //arrange
            Mock<IPersistence> persistenceMock = new Mock<IPersistence>(MockBehavior.Strict);
            persistenceMock.Setup(p => p.IsStorageLocation(ContentId)).Returns(false);
            persistenceMock.Setup(p => p.IsFolder(ContentId)).Returns(false);
            persistenceMock.Setup(p => p.IsDocument(ContentId)).Returns(true);

            //act
            string actual = Utilities.DoForContentObject(persistenceMock.Object, ContentId, contentId => "storageLocation", contentId => "folder", contentId => "document");

            //assert
            Assert.AreEqual("document", actual);
        }

        [TestMethod(DisplayName = nameof(DoForContentObjectThrowsKeyNotFoundExceptionWhenTheContentIdIsUnknown))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void DoForContentObjectThrowsKeyNotFoundExceptionWhenTheContentIdIsUnknown()
        {
            //arrange
            Mock<IPersistence> persistenceMock = new Mock<IPersistence>(MockBehavior.Strict);
            persistenceMock.Setup(p => p.IsStorageLocation(ContentId)).Returns(false);
            persistenceMock.Setup(p => p.IsFolder(ContentId)).Returns(false);
            persistenceMock.Setup(p => p.IsDocument(ContentId)).Returns(false);

            //act
            void Act() => Utilities.DoForContentObject<string>(persistenceMock.Object, ContentId, contentId => "storageLocation", contentId => "folder", contentId => "document");

            //assert
            Assert.ThrowsExactly<KeyNotFoundException>(Act);
        }

        [TestMethod(DisplayName = nameof(DoForContentObjectWithoutReturnValueInvokesTheMatchingAction))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void DoForContentObjectWithoutReturnValueInvokesTheMatchingAction()
        {
            //arrange
            Mock<IPersistence> persistenceMock = new Mock<IPersistence>(MockBehavior.Strict);
            persistenceMock.Setup(p => p.IsStorageLocation(ContentId)).Returns(false);
            persistenceMock.Setup(p => p.IsFolder(ContentId)).Returns(true);
            string? invokedFor = null;

            //act
            Utilities.DoForContentObject(persistenceMock.Object, ContentId, contentId => invokedFor = "storageLocation", contentId => invokedFor = "folder", contentId => invokedFor = "document");

            //assert
            Assert.AreEqual("folder", invokedFor);
        }

        [TestMethod(DisplayName = nameof(StringToLanguagesListReturnsAnEmptySetForNull))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void StringToLanguagesListReturnsAnEmptySetForNull()
        {
            //arrange
            //act
            ISet<string> actual = Utilities.StringToLanguagesList(null!);

            //assert
            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod(DisplayName = nameof(StringToLanguagesListReturnsAnEmptySetForAnEmptyString))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void StringToLanguagesListReturnsAnEmptySetForAnEmptyString()
        {
            //arrange
            //act
            ISet<string> actual = Utilities.StringToLanguagesList(string.Empty);

            //assert
            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod(DisplayName = nameof(StringToLanguagesListReturnsASingleEntrySetForASingleLanguage))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void StringToLanguagesListReturnsASingleEntrySetForASingleLanguage()
        {
            //arrange
            //act
            ISet<string> actual = Utilities.StringToLanguagesList("eng");

            //assert
            CollectionAssert.AreEquivalent(new[] { "eng" }, actual.ToList());
        }

        [TestMethod(DisplayName = nameof(StringToLanguagesListSplitsCommaSeparatedLanguages))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void StringToLanguagesListSplitsCommaSeparatedLanguages()
        {
            //arrange
            //act
            ISet<string> actual = Utilities.StringToLanguagesList("eng,deu,fra");

            //assert
            CollectionAssert.AreEquivalent(new[] { "eng", "deu", "fra" }, actual.ToList());
        }

        [TestMethod(DisplayName = nameof(LanguagesListToStringReturnsAnEmptyStringForAnEmptySet))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LanguagesListToStringReturnsAnEmptyStringForAnEmptySet()
        {
            //arrange
            ISet<string> languages = new HashSet<string>();

            //act
            string actual = Utilities.LanguagesListToString(languages);

            //assert
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod(DisplayName = nameof(LanguagesListToStringJoinsTheLanguagesWithACommaAndRoundTripsThroughStringToLanguagesList))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void LanguagesListToStringJoinsTheLanguagesWithACommaAndRoundTripsThroughStringToLanguagesList()
        {
            //arrange
            ISet<string> languages = new HashSet<string> { "eng", "deu" };

            //act
            string joined = Utilities.LanguagesListToString(languages);
            ISet<string> roundTripped = Utilities.StringToLanguagesList(joined);

            //assert
            CollectionAssert.AreEquivalent(languages.ToList(), roundTripped.ToList());
        }
    }
}
