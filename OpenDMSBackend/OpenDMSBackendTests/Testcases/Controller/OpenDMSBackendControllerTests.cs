using GRYLibrary.Core.APIServer.CommonDBTypes;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.OtherServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using OpenDMSBackend.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
            GRYLibrary.Core.APIServer.CommonDBTypes.User user = new GRYLibrary.Core.APIServer.CommonDBTypes.User() { Id = userId };


            ISet<string> additionalOCRLanguages = new HashSet<string>() { "deu", "eng" };
            authenticationServiceMock.Setup(mock => mock.GetUser(userId)).Returns(user);
            authenticationServiceMock.Setup(mock => mock.GetBaseRoleOfAllUser()).Returns(role);
            businessServiceMock.Setup(mock => mock.AddDocument(userId, title, containerId, filename, content, role, new HashSet<string>(additionalOCRLanguages))).Returns(documentIde);
            OpenDMSBackend.Core.Controller.OpenDMSBackendController controller = new OpenDMSBackend.Core.Controller.OpenDMSBackendController(businessServiceMock.Object, authenticationServiceMock.Object, timeService);

            List<Claim> claims = new List<Claim>
            {
                new Claim( "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",userId)
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
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

        [TestMethod]
        public void TestGetMetadataFields()
        {
            // arrange
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            ITimeService timeService = new TimeService();
            Mock<IBusinessLogicService> businessServiceMock = new Mock<IBusinessLogicService>(MockBehavior.Strict);
            string userId = "userid";
            string storageLocationId = "storageLocationId";
            GRYLibrary.Core.APIServer.CommonDBTypes.User user = new GRYLibrary.Core.APIServer.CommonDBTypes.User() { Id = userId };
            MetadataFieldDefinition[] definitions = new MetadataFieldDefinition[]
            {
                new MetadataFieldDefinition("field1", storageLocationId, "sender", MetadataFieldType.String)
            };

            authenticationServiceMock.Setup(mock => mock.GetUser(userId)).Returns(user);
            businessServiceMock.Setup(mock => mock.GetMetadataFields(userId, storageLocationId)).Returns(definitions);
            OpenDMSBackend.Core.Controller.OpenDMSBackendController controller = new OpenDMSBackend.Core.Controller.OpenDMSBackendController(businessServiceMock.Object, authenticationServiceMock.Object, timeService);

            List<Claim> claims = new List<Claim>
            {
                new Claim( "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",userId)
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            // act
            IActionResult actualResult = controller.GetMetadataFields(storageLocationId);

            // assert
            OkObjectResult okObjectResult = actualResult as OkObjectResult;
            Assert.IsNotNull(okObjectResult);
            List<MetadataFieldDefinitionDTO> returnedFields = ((IEnumerable<MetadataFieldDefinitionDTO>)okObjectResult.Value).ToList();
            Assert.AreEqual(1, returnedFields.Count);
            Assert.AreEqual("field1", returnedFields[0].Id);
            businessServiceMock.Verify(mock => mock.GetMetadataFields(userId, storageLocationId), Times.Once());
            businessServiceMock.VerifyNoOtherCalls();
            authenticationServiceMock.Verify(mock => mock.GetUser(userId), Times.Once());
            authenticationServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestDefineMetadataFieldPropagatesAuthorizationFailure()
        {
            // arrange: the business-logic-layer rejects the request because the caller is not a moderator of the storage-location.
            // The controller must propagate this failure unchanged (no swallowing, no wrapping, no leaking of internal details) so that
            // the global exception-handler maps it to a 403-response.
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            ITimeService timeService = new TimeService();
            Mock<IBusinessLogicService> businessServiceMock = new Mock<IBusinessLogicService>(MockBehavior.Strict);
            string userId = "userid";
            string storageLocationId = "storageLocationId";
            GRYLibrary.Core.APIServer.CommonDBTypes.User user = new GRYLibrary.Core.APIServer.CommonDBTypes.User() { Id = userId };
            MetadataFieldDefinitionCreationDTO creation = new MetadataFieldDefinitionCreationDTO("sender", "String");

            authenticationServiceMock.Setup(mock => mock.GetUser(userId)).Returns(user);
            businessServiceMock.Setup(mock => mock.DefineMetadataField(userId, storageLocationId, creation.Name, MetadataFieldType.String))
                .Throws(new GRYLibrary.Core.Exceptions.NotAuthorizedException($"Only a moderator of '{storageLocationId}' may manage its permissions."));
            OpenDMSBackend.Core.Controller.OpenDMSBackendController controller = new OpenDMSBackend.Core.Controller.OpenDMSBackendController(businessServiceMock.Object, authenticationServiceMock.Object, timeService);

            List<Claim> claims = new List<Claim>
            {
                new Claim( "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",userId)
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            // act & assert
            bool threw = false;
            try
            {
                controller.DefineMetadataField(storageLocationId, creation);
            }
            catch (GRYLibrary.Core.Exceptions.NotAuthorizedException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "The controller must propagate the authorization-failure raised by the business-logic-service.");
            businessServiceMock.Verify(mock => mock.DefineMetadataField(userId, storageLocationId, creation.Name, MetadataFieldType.String), Times.Once());
            businessServiceMock.VerifyNoOtherCalls();
            authenticationServiceMock.Verify(mock => mock.GetUser(userId), Times.Once());
            authenticationServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestDefineMetadataFieldRejectsInvalidType()
        {
            // arrange: an unsupported field-type must be rejected before any change is attempted, so the business-logic-service must never be called.
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            ITimeService timeService = new TimeService();
            Mock<IBusinessLogicService> businessServiceMock = new Mock<IBusinessLogicService>(MockBehavior.Strict);
            string userId = "userid";
            string storageLocationId = "storageLocationId";
            GRYLibrary.Core.APIServer.CommonDBTypes.User user = new GRYLibrary.Core.APIServer.CommonDBTypes.User() { Id = userId };
            MetadataFieldDefinitionCreationDTO creation = new MetadataFieldDefinitionCreationDTO("sender", "NotAValidType");

            authenticationServiceMock.Setup(mock => mock.GetUser(userId)).Returns(user);
            OpenDMSBackend.Core.Controller.OpenDMSBackendController controller = new OpenDMSBackend.Core.Controller.OpenDMSBackendController(businessServiceMock.Object, authenticationServiceMock.Object, timeService);

            List<Claim> claims = new List<Claim>
            {
                new Claim( "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",userId)
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuth");
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };

            // act & assert
            bool threw = false;
            try
            {
                controller.DefineMetadataField(storageLocationId, creation);
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "An unsupported metadata-field-type must be rejected with a bad-request-error.");
            businessServiceMock.VerifyNoOtherCalls();
        }
    }
}
