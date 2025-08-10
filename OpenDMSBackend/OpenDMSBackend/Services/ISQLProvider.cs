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
        string GetScriptGetAmountOfDocuments();
        string GetScriptGetUserByName();
        string GetScriptAddAccessToken();
        string GetScriptGetAccessToken();
        string GetScriptGetDocument();
        string GetScriptIsFolder();
        string GetScriptIsStorageLocation();
        string GetScriptIsDocument();
        string GetScriptGetUserById();
        string GetScriptGetAllAccessTokenForUser();
        string GetScriptRemoveAccessToken();
        string GetScriptGetParentIdOfContainee();
        string GetScriptGetAllStorageLocations();
        string GetScriptGetStorageLocation();
        string GetScriptGetFolder();
        string GetScriptGetIdFromReadableId();
        string GetScriptSearch();
        string GetScriptGetDocumentPreview();
    }
}
