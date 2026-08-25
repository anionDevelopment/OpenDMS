using GRYLibrary.Core.Misc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackend.Core.Services;
using OpenDMSBackend.Tests.TestUtilities;
using System;
using System.Net;
using System.Net.Http;

namespace OpenDMSBackend.Tests.Testcases.Controller
{
    [TestClass]
    [Ignore("ignored because otherwise the testcoverage remains 0% for unknown reasons.")]
    public class MaintenanceControllerTests
    {
        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.IntegrationTest))]
        public void HealthCheckIsWorking()
        {
            lock (OpenDMSBackend.Tests.TestUtilities.Utilities.LockForTests)
            {
                // arrange
                Mock<IExampleDataCreator> exampleDataCreatorMock = new Mock<IExampleDataCreator>();
                using (IntegrationTestFramework testFramework = new IntegrationTestFramework(new IntegrationTestConfiguration((functionalInformation) =>
                {
                    ServiceDescriptor exampleDataCreatorMockDescriptor =
                        new ServiceDescriptor(
                            typeof(IExampleDataCreator),
                            (_) => exampleDataCreatorMock.Object,
                            ServiceLifetime.Singleton);
                    functionalInformation.WebApplicationBuilder.Services.Replace(exampleDataCreatorMockDescriptor);
                }, true), true))
                {

                    string HealthCheckUrl = $"{testFramework.GetServerURL()}/API/Other/Maintenance/HealthCheck";
                    using (HttpClient client = testFramework.GetClient())
                    {

                        // act
                        HttpResponseMessage response = client.GetAsync(HealthCheckUrl).WaitAndGetResult();

                        // assert
                        string contentString = response.Content.ReadAsStringAsync().WaitAndGetResult();
                        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"Got response-code \"{response.StatusCode}\". Response-body: \"{contentString}\"");
                    }
                }
            }
        }
    }
}
