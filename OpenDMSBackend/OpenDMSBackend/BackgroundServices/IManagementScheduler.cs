using GRYLibrary.Core.APIServer.BaseServices;

namespace OpenDMSBackend.Core.BackgroundServices
{
    /// <summary>
    /// This service does regulary overhead like importing new document from defined locations and doing scheduled deletion-tasks.
    /// </summary>
    public interface IManagementScheduler : IIteratingBackgroundService
    {
    }
}
