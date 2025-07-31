using GRYLibrary.Core.APIServer.BaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDMSBackend.Core.BackgroundServices
{
    /// <summary>
    /// This service does regulary overhead like importing new document from defined locations and doing scheduled deletion-tasks.
    /// </summary>
    public interface IManagementScheduler : IIteratingBackgroundService
    {
    }
}
