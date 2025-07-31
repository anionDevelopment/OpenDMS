using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDMSBackend.Core.Configuration
{
    public class ImportDefinition
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public string SourceLocation { get; set; }
        public string TargetCollection { get; set; }
        public string AdaptDocumentScriptBody { get; set; }
    }
}
