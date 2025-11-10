using CommandLine;
using GRYLibrary.Core.APIServer.Verbs;

namespace OpenDMSBackend.Core.Configuration
{
    public class CommandlineParameter : RunServer
    {

        [Option(nameof(InitialAdminPassword), Required = false)]
        public string? InitialAdminPassword { get; set; }

        [Option(nameof(InitialDatabaseType), Required = false)]
        public string? InitialDatabaseType { get; set; }

        [Option(nameof(InitialDatabaseConnectionString), Required = false)]
        public string? InitialDatabaseConnectionString { get; set; }

        [Option(nameof(InitialOCRDataServiceAddress), Required = false)]
        public string? InitialOCRDataServiceAddress { get; set; }

        [Option(nameof(InitialOCRDataServiceAPIKey), Required = false)]
        public string? InitialOCRDataServiceAPIKey { get; set; }
    }
}
