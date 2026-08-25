namespace OpenDMSBackend.Core.Services
{
    public class GeneralResourceLoader : GRYLibrary.Core.APIServer.Services.Res.GeneralResourceLoader
    {
        public GeneralResourceLoader() : base("OpenDMSBackend.Core.Resources", typeof(Program).Assembly) { }
    }
}
