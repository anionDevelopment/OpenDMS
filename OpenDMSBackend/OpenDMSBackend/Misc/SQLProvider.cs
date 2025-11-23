using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Services;

namespace OpenDMSBackend.Core.Misc
{
    public abstract class SQLProvider : AbstractSQLProvider, ISQLProvider
    {
        public SQLProvider(string databaseType) : base($"OpenDMSBackend.Core.Resources.Database.{databaseType}.Statements") { }

        public string GetScriptResetDatabase()
        {
            return this.LoadSQLScript("ResetDatabase");
        }

        public string GetScriptRoleExists()
        {
            return this.LoadSQLScript("RoleExists");
        }

        public string GetScriptAddDocument()
        {
            return this.LoadSQLScript("AddDocument");
        }
        public string GetScriptUpdateDocument()
        {
            return this.LoadSQLScript("UpdateDocument");
        }

        public string GetScriptAddRole()
        {
            return this.LoadSQLScript("AddRole");
        }

        public string GetScriptGetAllRoles()
        {
            return this.LoadSQLScript("GetAllRoles");
        }

        public string GetScriptUpdateRole()
        {
            return this.LoadSQLScript("UpdateRole");
        }

        public string GetScriptUserWithNameExists()
        {
            return this.LoadSQLScript("UserWithNameExists");
        }

        public string GetScriptAddUser()
        {
            return this.LoadSQLScript("AddUser");
        }

        public string GetScriptUserHasRole()
        {
            return this.LoadSQLScript("UserHasRole");
        }

        public string GetScriptAddRoleToUser()
        {
            return this.LoadSQLScript("AddRoleToUser");
        }

        public string GetScriptUserWithIdExists()
        {
            return this.LoadSQLScript("UserWithIdExists");
        }

        public string GetScriptGetAmountOfDocuments()
        {
            return this.LoadSQLScript("GetAmountOfDocuments");
        }

        public string GetScriptGetUserByName()
        {
            return this.LoadSQLScript("GetUserByName");
        }

        public string GetScriptAddAccessToken()
        {
            return this.LoadSQLScript("AddAccessToken");
        }

        public string GetScriptGetAccessToken()
        {
            return this.LoadSQLScript("GetAccessToken");
        }

        public string GetScriptGetDocument()
        {
            return this.LoadSQLScript("GetDocument");
        }

        public string GetScriptIsFolder()
        {
            return this.LoadSQLScript("IsFolder");
        }

        public string GetScriptIsStorageLocation()
        {
            return this.LoadSQLScript("IsStorageLocation");
        }

        public string GetScriptIsDocument()
        {
            return this.LoadSQLScript("IsDocument");
        }

        public string GetScriptGetUserById()
        {
            return this.LoadSQLScript("GetUserById");
        }

        public string GetScriptGetAllAccessTokenForUser()
        {
            return this.LoadSQLScript("GetAllAccessTokenForUser");
        }

        public string GetScriptRemoveAccessToken()
        {
            return this.LoadSQLScript("RemoveAccessToken");
        }

        public string GetScriptGetParentIdOfContainee()
        {
            return this.LoadSQLScript("GetParentIdOfContainee");
        }

        public string GetScriptGetAllStorageLocationIds()
        {
            return this.LoadSQLScript("GetAllStorageLocationIds");
        }

        public string GetScriptGetStorageLocation()
        {
            return this.LoadSQLScript("GetStorageLocation");
        }

        public string GetScriptGetFolder()
        {
            return this.LoadSQLScript("GetFolder");
        }

        public string GetScriptGetIdFromReadableId()
        {
            return this.LoadSQLScript("GetIdFromReadableId");
        }

        public string GetScriptSearch()
        {
            return this.LoadSQLScript("Search");
        }

        public string GetScriptGetDocumentPreview()
        {
            return this.LoadSQLScript("GetDocumentPreview");
        }

        public string GetScriptDeleteInheritedRoles()
        {
            return this.LoadSQLScript("DeleteInheritedRoles");
        }

        public string GetScriptAddInheritedRole()
        {
            return this.LoadSQLScript("AddInheritedRole");
        }

        public string GetScriptGetRoleByName()
        {
            return this.LoadSQLScript("GetRoleByName");
        }

        public string GetScriptGetRoleById()
        {
            return this.LoadSQLScript("GetRoleById");
        }

        public string GetScriptGetInheritedRoles()
        {
            return this.LoadSQLScript("GetInheritedRoles");
        }

        public string GetScriptAddStorageLocation()
        {
            return this.LoadSQLScript("AddStorageLocation");
        }

        public string GetScriptAddFolder()
        {
            return this.LoadSQLScript("AddFolder");
        }

        public string GetScriptAddOrUpdateOwnerOfStorageLocation()
        {
            return this.LoadSQLScript("AddOrUpdateOwnerOfStorageLocation");
        }

        public string GetScriptSetParentOfContainee()
        {
            return this.LoadSQLScript("SetParentOfContainee");
        }

        public string GetScriptGetTagsOfDocument()
        {
            return this.LoadSQLScript("GetTagsOfDocument");
        }

        public string GetScriptGetTag()
        {
            return this.LoadSQLScript("GetTag");
        }

        public string GetScriptGetAllDocumentIds()
        {
            return this.LoadSQLScript("GetAllDocumentIds");
        }

        public string GetScriptGetAllTags()
        {
            return this.LoadSQLScript("GetAllTags");
        }

        public string GetScriptAddTag()
        {
            return this.LoadSQLScript("AddTag");
        }

        public string GetScriptGetRolesOfUser()
        {
            return this.LoadSQLScript("GetRolesOfUser");
        }
        public string GetScriptGetStorageLocationIdOfContainee()
        {
            return this.LoadSQLScript("GetStorageLocationIdOfContainee");
        }

        public string GetScriptGetContentOfContainer()
        {
            return this.LoadSQLScript("GetContentOfContainer");
        }
    }
}
