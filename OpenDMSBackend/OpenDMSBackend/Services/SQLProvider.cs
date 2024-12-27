using GRYLibrary.Core.Misc;

namespace OpenDMSBackend.Core.Services
{
    public class SQLProvider : AbstractSQLProvider,ISQLProvider
    {
        public SQLProvider() :base("OpenDMSBackend.Core.Resources.Database.SQL") { }

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
    }
}
