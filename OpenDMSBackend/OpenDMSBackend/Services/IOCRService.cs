using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDMSBackend.Core.Services
{
    public interface IOCRService
    {
       public string GetOCRContent(byte[] documentContent);
    }
}
