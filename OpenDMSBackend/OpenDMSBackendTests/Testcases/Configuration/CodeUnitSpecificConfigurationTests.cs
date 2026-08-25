using GRYLibrary.Core.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenDMSBackend.Core.Configuration;

namespace OpenDMSBackend.Tests.Testcases.Configuration
{
    [TestClass]
    public class CodeUnitSpecificConfigurationTests
    {
        [TestMethod(DisplayName = nameof(CodeUnitSpecificConfigurationRegistrationIsAllowed))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
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

        [TestMethod(DisplayName = nameof(CodeUnitSpecificConfigurationLoginIsAllowed))]
        [TestProperty(nameof(GRYLibrary.Core.Misc.TestKind), nameof(GRYLibrary.Core.Misc.TestKind.UnitTest))]
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
