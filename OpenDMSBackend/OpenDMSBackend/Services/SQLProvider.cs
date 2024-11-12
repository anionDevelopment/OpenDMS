using System.Collections.Generic;
using System.IO;

namespace OpenDMSBackend.Core.Services
{
    public class SQLProvider : ISQLProvider
    {
        private IDictionary<string, string> _ScriptCache = new Dictionary<string, string>();

        private string LoadSQLScript(string sqlFileName)
        {
            if (!this._ScriptCache.ContainsKey(sqlFileName))
            {
                this.LoadScriptToCache(sqlFileName);
            }
            return this._ScriptCache[sqlFileName];
        }

        private void LoadScriptToCache(string sqlFileName)
        {
            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream($"OpenDMSBackend.Core.Database.SQL.{sqlFileName}.sql"))
            using (StreamReader reader = new StreamReader(stream))
            {
                this._ScriptCache[sqlFileName] = reader.ReadToEnd();
            }
        }

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
