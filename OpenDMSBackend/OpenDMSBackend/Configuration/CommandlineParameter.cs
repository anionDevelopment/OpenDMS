using CommandLine;
using GRYLibrary.Core.APIServer.Verbs;

namespace OpenDMSBackend.Core.Configuration
{
    public class CommandlineParameter : RunServer
    {

        [Option(nameof(InitialAdminPassword), Required = false)]
        public string? InitialAdminPassword { get; set; }
    }
}
