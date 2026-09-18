using GRYLibrary.Core.Misc;
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
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
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
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
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
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
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
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
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

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void TestCreateTagPassesTheParsedColor()
        {
            // arrange: the color is transferred as a six-digit hexadecimal rgb-value, which the controller has to turn into the color the business-logic works with.
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            ITimeService timeService = new TimeService();
            Mock<IBusinessLogicService> businessServiceMock = new Mock<IBusinessLogicService>(MockBehavior.Strict);
            string userId = "userid";
            string createdTagId = "createdTagId";
            GRYLibrary.Core.APIServer.CommonDBTypes.User user = new GRYLibrary.Core.APIServer.CommonDBTypes.User() { Id = userId };
            ExtendedColor expectedColor = new ExtendedColor(198, 40, 40);
            TagCreationDTO creation = new TagCreationDTO("Invoice", "C62828");

            authenticationServiceMock.Setup(mock => mock.GetUser(userId)).Returns(user);
            businessServiceMock.Setup(mock => mock.CreateTag(userId, "Invoice", expectedColor)).Returns(createdTagId);
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
            IActionResult actualResult = controller.CreateTag(creation);

            // assert
            OkObjectResult okObjectResult = actualResult as OkObjectResult;
            Assert.IsNotNull(okObjectResult);
            Assert.AreEqual(createdTagId, (string)okObjectResult.Value);
            businessServiceMock.Verify(mock => mock.CreateTag(userId, "Invoice", expectedColor), Times.Once());
            businessServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void TestCreateTagRejectsInvalidColorCode()
        {
            // arrange: a malformed color must be rejected before any change is attempted, so the business-logic-service must never be called.
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            ITimeService timeService = new TimeService();
            Mock<IBusinessLogicService> businessServiceMock = new Mock<IBusinessLogicService>(MockBehavior.Strict);
            string userId = "userid";
            GRYLibrary.Core.APIServer.CommonDBTypes.User user = new GRYLibrary.Core.APIServer.CommonDBTypes.User() { Id = userId };
            TagCreationDTO creation = new TagCreationDTO("Invoice", "not-a-color");

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
                controller.CreateTag(creation);
            }
            catch (GRYLibrary.Core.Exceptions.BadRequestException)
            {
                threw = true;
            }
            Assert.IsTrue(threw, "A color which is not a six-digit hexadecimal rgb-value must be rejected with a bad-request-error.");
            businessServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
        public void TestAssignTagDelegatesToTheBusinessLogic()
        {
            // arrange
            Mock<IAuthenticationService> authenticationServiceMock = new Mock<IAuthenticationService>(MockBehavior.Strict);
            ITimeService timeService = new TimeService();
            Mock<IBusinessLogicService> businessServiceMock = new Mock<IBusinessLogicService>(MockBehavior.Strict);
            string userId = "userid";
            string documentId = "documentId";
            string tagId = "tagId";
            GRYLibrary.Core.APIServer.CommonDBTypes.User user = new GRYLibrary.Core.APIServer.CommonDBTypes.User() { Id = userId };

            authenticationServiceMock.Setup(mock => mock.GetUser(userId)).Returns(user);
            businessServiceMock.Setup(mock => mock.AssignTag(userId, documentId, tagId));
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
            IActionResult actualResult = controller.AssignTag(documentId, tagId);

            // assert
            Assert.IsNotNull(actualResult as OkResult);
            businessServiceMock.Verify(mock => mock.AssignTag(userId, documentId, tagId), Times.Once());
            businessServiceMock.VerifyNoOtherCalls();
        }
    }
}
