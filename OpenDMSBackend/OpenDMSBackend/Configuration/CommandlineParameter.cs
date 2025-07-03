using CommandLine;
using GRYLibrary.Core.APIServer.Verbs;

namespace OpenDMSBackend.Core.Configuration
{
    public class CommandlineParameter : RunServer
    {

        [Option(nameof(InitialAdminPassword), Required = true)]
        public string? InitialAdminPassword { get; set; }

        [Option(nameof(InitialDatabaseType), Required = true)]
        public string? InitialDatabaseType { get; set; }

        [Option(nameof(InitialDatabaseConnectionString), Required = true)]
        public string? InitialDatabaseConnectionString { get; set; }
    }
}
