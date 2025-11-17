namespace OpenDMSBackend.Core.Services
{
    public interface ISQLProvider
    {
        string GetScriptAddRoleToUser();
        string GetScriptAddUser();
        string GetScriptAddRole();
        string GetScriptGetAllRoles();
        string GetScriptAddDocument();
        string GetScriptDeleteInheritedRoles();
        string GetScriptAddInheritedRole();
        string GetScriptGetRoleByName();
        string GetScriptGetRoleById();
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
        string GetScriptGetInheritedRoles();
        string GetScriptAddStorageLocation();
        string GetScriptAddFolder();
        string GetScriptAddOrUpdateOwnerOfStorageLocation();
        string GetScriptSetParentOfContainee();
        string GetScriptGetTagsOfDocument();
        string GetScriptGetTag();
        string GetScriptGetAllDocumentIds();
        string GetScriptGetAllTags();
        string GetScriptAddTag();
    }
}
