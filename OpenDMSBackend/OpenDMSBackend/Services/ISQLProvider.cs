namespace OpenDMSBackend.Core.Services
{
    public interface ISQLProvider
    {
        string GetScriptAddRoleToUser();
        string GetScriptAddUser();
        string GetScriptInsertRole();
        string GetScriptGetAllRoles();
        string GetScriptAddDocument();
        string GetScriptResetDatabase();
        string GetScriptRoleExists();
        string GetScriptUpdateRole();
        string GetScriptUserHasRole();
        string GetScriptUserWithIdExists();
        string GetScriptUserWithNameExists();
    }
}
