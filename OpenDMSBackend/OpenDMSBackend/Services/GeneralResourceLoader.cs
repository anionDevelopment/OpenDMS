using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDMSBackend.Core.Services
{
    public class GeneralResourceLoader : GRYLibrary.Core.APIServer.Services.Res.GeneralResourceLoader
    {
        public GeneralResourceLoader() : base("OpenDMSBackend.Core.Resources") { }
    }
}
