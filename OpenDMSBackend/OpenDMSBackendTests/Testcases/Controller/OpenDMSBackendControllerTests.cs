using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace OpenDMSBackend.Tests.Testcases.Controller
{
    [TestClass]
    public class OpenDMSBackendControllerTests
    {
        [TestMethod]
        public void TestAddDocument()
        {
            // arrange
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            ITimeService timeService = new TimeService();
            Mock<IBusinessLogicService> businessServiceMock = new Mock<IBusinessLogicService>(MockBehavior.Strict);
            string documentIde = Guid.NewGuid().ToString();
            string role = "role";
            string userId = "userid";
            string title = "title";
            string containerId = "containerId";
            string filename = "filename";
            byte[] content = new byte[] { 1, 2, 3 };
            User user = new User() { Id = userId };


            ISet<string> additionalOCRLanguages = new HashSet<string>() { "deu", "eng" };
            authenticationServiceMock.Setup(mock => mock.GetUser(userId)).Returns(user);
            authenticationServiceMock.Setup(mock => mock.GetBaseRoleOfAllUser()).Returns(role);
            businessServiceMock.Setup(mock => mock.AddDocument(userId, title, containerId, filename, content, role, new HashSet<string>(additionalOCRLanguages))).Returns(documentIde);
            OpenDMSBackend.Core.Controller.OpenDMSBackendController controller = new OpenDMSBackend.Core.Controller.OpenDMSBackendController(businessServiceMock.Object, authenticationServiceMock.Object, timeService);

            var claims = new List<Claim>
            {
                new Claim( "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            // act
            IActionResult actualResult = controller.AddDocument(content, containerId, filename, title, additionalOCRLanguages);

            // assert
            OkObjectResult okObjectResult = actualResult as OkObjectResult;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(documentIde, (string)okObjectResult.Value);
            businessServiceMock.Verify(mock => mock.AddDocument(userId, title, containerId, filename, content, role, new HashSet<string>(additionalOCRLanguages)), Times.Once());
            businessServiceMock.VerifyNoOtherCalls();
            authenticationServiceMock.Verify(mock => mock.GetBaseRoleOfAllUser(), Times.Once());
            authenticationServiceMock.Verify(mock => mock.GetUser(userId),Times.Once());
            authenticationServiceMock.VerifyNoOtherCalls();
        }
        //TODO add testcase that you get a 403-response when you try to load a document without having the requried permissions.
    }
}
