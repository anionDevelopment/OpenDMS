using GRYLibrary.Core.APIServer.Settings;
using System;

namespace OpenDMSBackend.Tests.TestUtilities
{
    public class IntegrationTestConfiguration
    {
        public bool RunInOwnThread { get; set; }
        public Action<FunctionalInformation<OpenDMSBackend.Core.Constants.CodeUnitSpecificConstants, OpenDMSBackend.Core.Configuration.CodeUnitSpecificConfiguration, OpenDMSBackend.Core.Configuration.CommandlineParameter>>? SetupMocks { get; set; }
        public IntegrationTestConfiguration(Action<FunctionalInformation<OpenDMSBackend.Core.Constants.CodeUnitSpecificConstants, OpenDMSBackend.Core.Configuration.CodeUnitSpecificConfiguration, OpenDMSBackend.Core.Configuration.CommandlineParameter>>? setupMocks = null, bool runInOwnThread = false)
        {
            this.SetupMocks = setupMocks;
            this.RunInOwnThread = runInOwnThread;
        }
    }
}
