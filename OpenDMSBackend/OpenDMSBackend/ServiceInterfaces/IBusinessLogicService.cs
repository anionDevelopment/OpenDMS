using OpenDMSBackend.Core.Model;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public interface IBusinessLogicService
    {
        public void AddDocument(Document document);
        string Register(string username, string initialAdminPassword);
        bool UserWithNameExists(string username);
    }
}
