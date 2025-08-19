using ExtendedXmlSerializer;
using GRYLibrary.Core.Misc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace OpenDMSBackend.Tests.Testcases.Controller
{
    [TestClass]
    public class OpenDMSBackendControllerTests
    {
        [TestMethod]
        [TestProperty(nameof(TestKind), nameof(TestKind.IntegrationTest))]
        public void AddDocumentTest()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.IntegrationTestLock)
            {
                // arrange
                string documentId = "documentId";
                string requesterUserId = "requesterUserId";
                string? title = "title";
                string originalFilename = "originalFilename";
                byte[] content = new byte[] { 3, 5 };
                string groupOfBusinessOwner = "groupOfBusinessOwner";
                Mock<IExampleDataCreator> exampleDataCreatorMock = new Mock<IExampleDataCreator>();
                string ocrLanguages = "deu+eng";
                using IntegrationTestFramework testFramework = new IntegrationTestFramework(new IntegrationTestConfiguration((functionalInformation) =>
                {
                    ServiceDescriptor exampleDataCreatorMockDescriptor =
                        new ServiceDescriptor(
                            typeof(IExampleDataCreator),
                            (_) => exampleDataCreatorMock.Object,
                            ServiceLifetime.Singleton);
                    functionalInformation.WebApplicationBuilder.Services.Replace(exampleDataCreatorMockDescriptor);
                }, false));
                var user = testFramework.GetUser();
                string containerId = testFramework.BusinessLogicService.AddStorageLocation(user.Id, "storageLocation");

                string addUrl = $"{testFramework.GetServerURL()}{OpenDMSBackend.Core.Controller.OpenDMSBackendController.ControllerRoute}/{nameof(OpenDMSBackend.Core.Controller.OpenDMSBackendController.AddDocument)}/{containerId}?filename={originalFilename}&title={title}&additionalOCRLanguages={ocrLanguages}";
                using HttpClient client = testFramework.GetClient(user);
                using ByteArrayContent byteArrayContent = new ByteArrayContent(content);
                byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                // act
                HttpResponseMessage response = client.PostAsync(addUrl, byteArrayContent).WaitAndGetResult();

                // assert
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                string contentString = response.Content.ReadAsStringAsync().WaitAndGetResult();
                bool isValidGuid = Guid.TryParse(contentString, out Guid parsedGuid);
                Assert.IsTrue(isValidGuid);
                Assert.AreNotEqual(Guid.Empty, parsedGuid);
            }
        }
        //TODO add testcase that you get a 403-response when you try to load a document without having the requried permissions.
    }
}
