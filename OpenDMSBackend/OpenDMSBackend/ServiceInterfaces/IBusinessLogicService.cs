using OpenDMSBackend.Core.Model;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public interface IBusinessLogicService
    {
        public void AddDocument(Document document);
        string Register(string adminUsername, string initialAdminPassword);
        bool UserExists(string adminUserName);
        bool UserExistsByName(string adminUsername);
    }
}
