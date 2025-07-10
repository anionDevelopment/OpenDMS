using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Services;

namespace OpenDMSBackend.Core.Miscellaneous
{
    public abstract class SQLProvider : AbstractSQLProvider, ISQLProvider
    {
        public SQLProvider(string databaseType, IGRYLog log) : base($"OpenDMSBackend.Core.Resources.Database.{databaseType}.Statements", log) { }

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

        public string GetScriptInsertRole()
        {
            return this.LoadSQLScript("InsertRole");
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

        public string GetScriptGetAllStorageLocations()
        {
            return this.LoadSQLScript("GetAllStorageLocations");
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
    }
}
