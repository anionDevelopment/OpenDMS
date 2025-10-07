using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Configuration;

namespace OpenDMSBackend.Tests.Testcases.Configuration
{
    [TestClass]
    public class CodeUnitSpecificConfigurationTests
    {
        [TestMethod(nameof(CodeUnitSpecificConfigurationRegistrationIsAllowed))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void CodeUnitSpecificConfigurationRegistrationIsAllowed()
        {
            //arrange
            bool expected = true;
            CodeUnitSpecificConfiguration configuration = new CodeUnitSpecificConfiguration();

            //act
            configuration.RegistrationIsEnabled = expected;
            bool actual = configuration.RegistrationIsEnabled;

            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod(nameof(CodeUnitSpecificConfigurationLoginIsAllowed))]
        [TestProperty(nameof(TestKind), nameof(TestKind.UnitTest))]
        public void CodeUnitSpecificConfigurationLoginIsAllowed()
        {
            //arrange
            bool expected = true;
            CodeUnitSpecificConfiguration configuration = new CodeUnitSpecificConfiguration();

            //act
            configuration.LoginIsEnabled = expected;
            bool actual = configuration.LoginIsEnabled;

            //assert
            Assert.AreEqual(expected, actual);
        }
    }
}
