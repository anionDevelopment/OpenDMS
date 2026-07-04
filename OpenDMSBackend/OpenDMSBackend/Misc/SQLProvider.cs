using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Services;

namespace OpenDMSBackend.Core.Misc
{
    public abstract class SQLProvider : AbstractSQLProvider, ISQLProvider
    {
        /// <summary>Initializes the SQL provider for the specified database type.</summary>
        /// <param name="databaseType">The database type identifier used to locate the embedded SQL script folder (e.g. "MariaDB" or "PostgreSQL").</param>
        public SQLProvider(string databaseType) : base($"OpenDMSBackend.Core.Resources.Database.{databaseType}.Statements") { }

        /// <inheritdoc />
        public string GetScriptResetDatabase()
        {
            return this.LoadSQLScript("ResetDatabase");
        }

        /// <inheritdoc />
        public string GetScriptRoleExists()
        {
            return this.LoadSQLScript("RoleExists");
        }

        /// <inheritdoc />
        public string GetScriptAddDocument()
        {
            return this.LoadSQLScript("AddDocument");
        }

        /// <inheritdoc />
        public string GetScriptUpdateDocument()
        {
            return this.LoadSQLScript("UpdateDocument");
        }

        /// <inheritdoc />
        public string GetScriptSoftDeleteDocument()
        {
            return this.LoadSQLScript("SoftDeleteDocument");
        }

        /// <inheritdoc />
        public string GetScriptSetAISummary()
        {
            return this.LoadSQLScript("SetAISummary");
        }

        /// <inheritdoc />
        public string GetScriptGetSetting()
        {
            return this.LoadSQLScript("GetSetting");
        }

        /// <inheritdoc />
        public string GetScriptSetSetting()
        {
            return this.LoadSQLScript("SetSetting");
        }

        /// <inheritdoc />
        public string GetScriptAddDocumentVersionLink()
        {
            return this.LoadSQLScript("AddDocumentVersionLink");
        }

        /// <inheritdoc />
        public string GetScriptGetSupersededDocumentIds()
        {
            return this.LoadSQLScript("GetSupersededDocumentIds");
        }

        /// <inheritdoc />
        public string GetScriptGetPreviousVersionId()
        {
            return this.LoadSQLScript("GetPreviousVersionId");
        }

        /// <inheritdoc />
        public string GetScriptGetNextVersionId()
        {
            return this.LoadSQLScript("GetNextVersionId");
        }

        /// <inheritdoc />
        public string GetScriptAddRole()
        {
            return this.LoadSQLScript("AddRole");
        }

        /// <inheritdoc />
        public string GetScriptGetAllRoles()
        {
            return this.LoadSQLScript("GetAllRoles");
        }

        /// <inheritdoc />
        public string GetScriptUpdateRole()
        {
            return this.LoadSQLScript("UpdateRole");
        }

        /// <inheritdoc />
        public string GetScriptUserWithNameExists()
        {
            return this.LoadSQLScript("UserWithNameExists");
        }

        /// <inheritdoc />
        public string GetScriptAddUser()
        {
            return this.LoadSQLScript("AddUser");
        }

        /// <inheritdoc />
        public string GetScriptUserHasRole()
        {
            return this.LoadSQLScript("UserHasRole");
        }

        /// <inheritdoc />
        public string GetScriptAddRoleToUser()
        {
            return this.LoadSQLScript("AddRoleToUser");
        }

        /// <inheritdoc />
        public string GetScriptUserWithIdExists()
        {
            return this.LoadSQLScript("UserWithIdExists");
        }

        /// <inheritdoc />
        public string GetScriptGetAmountOfDocuments()
        {
            return this.LoadSQLScript("GetAmountOfDocuments");
        }

        /// <inheritdoc />
        public string GetScriptGetUserByName()
        {
            return this.LoadSQLScript("GetUserByName");
        }

        /// <inheritdoc />
        public string GetScriptAddAccessToken()
        {
            return this.LoadSQLScript("AddAccessToken");
        }

        /// <inheritdoc />
        public string GetScriptGetAccessToken()
        {
            return this.LoadSQLScript("GetAccessToken");
        }

        /// <inheritdoc />
        public string GetScriptGetDocument()
        {
            return this.LoadSQLScript("GetDocument");
        }

        /// <inheritdoc />
        public string GetScriptIsFolder()
        {
            return this.LoadSQLScript("IsFolder");
        }

        /// <inheritdoc />
        public string GetScriptIsStorageLocation()
        {
            return this.LoadSQLScript("IsStorageLocation");
        }

        /// <inheritdoc />
        public string GetScriptIsDocument()
        {
            return this.LoadSQLScript("IsDocument");
        }

        /// <inheritdoc />
        public string GetScriptGetUserById()
        {
            return this.LoadSQLScript("GetUserById");
        }

        /// <inheritdoc />
        public string GetScriptGetAllAccessTokenForUser()
        {
            return this.LoadSQLScript("GetAllAccessTokenForUser");
        }

        /// <inheritdoc />
        public string GetScriptRemoveAccessToken()
        {
            return this.LoadSQLScript("RemoveAccessToken");
        }

        /// <inheritdoc />
        public string GetScriptGetParentIdOfContainee()
        {
            return this.LoadSQLScript("GetParentIdOfContainee");
        }

        /// <inheritdoc />
        public string GetScriptGetAllStorageLocationIds()
        {
            return this.LoadSQLScript("GetAllStorageLocationIds");
        }

        /// <inheritdoc />
        public string GetScriptGetStorageLocation()
        {
            return this.LoadSQLScript("GetStorageLocation");
        }

        /// <inheritdoc />
        public string GetScriptGetFolder()
        {
            return this.LoadSQLScript("GetFolder");
        }

        /// <inheritdoc />
        public string GetScriptGetIdFromReadableId()
        {
            return this.LoadSQLScript("GetIdFromReadableId");
        }

        /// <inheritdoc />
        public string GetScriptSearch()
        {
            return this.LoadSQLScript("Search");
        }

        /// <inheritdoc />
        public string GetScriptGetDocumentPreview()
        {
            return this.LoadSQLScript("GetDocumentPreview");
        }

        /// <inheritdoc />
        public string GetScriptDeleteInheritedRoles()
        {
            return this.LoadSQLScript("DeleteInheritedRoles");
        }

        /// <inheritdoc />
        public string GetScriptAddInheritedRole()
        {
            return this.LoadSQLScript("AddInheritedRole");
        }

        /// <inheritdoc />
        public string GetScriptGetRoleByName()
        {
            return this.LoadSQLScript("GetRoleByName");
        }

        /// <inheritdoc />
        public string GetScriptGetRoleById()
        {
            return this.LoadSQLScript("GetRoleById");
        }

        /// <inheritdoc />
        public string GetScriptGetInheritedRoles()
        {
            return this.LoadSQLScript("GetInheritedRoles");
        }

        /// <inheritdoc />
        public string GetScriptAddStorageLocation()
        {
            return this.LoadSQLScript("AddStorageLocation");
        }

        /// <inheritdoc />
        public string GetScriptAddFolder()
        {
            return this.LoadSQLScript("AddFolder");
        }

        /// <inheritdoc />
        public string GetScriptAddOrUpdateOwnerOfStorageLocation()
        {
            return this.LoadSQLScript("AddOrUpdateOwnerOfStorageLocation");
        }

        /// <inheritdoc />
        public string GetScriptSetParentOfContainee()
        {
            return this.LoadSQLScript("SetParentOfContainee");
        }

        /// <inheritdoc />
        public string GetScriptGetTagsOfDocument()
        {
            return this.LoadSQLScript("GetTagsOfDocument");
        }

        /// <inheritdoc />
        public string GetScriptGetTag()
        {
            return this.LoadSQLScript("GetTag");
        }

        /// <inheritdoc />
        public string GetScriptGetAllDocumentIds()
        {
            return this.LoadSQLScript("GetAllDocumentIds");
        }

        /// <inheritdoc />
        public string GetScriptGetAllTags()
        {
            return this.LoadSQLScript("GetAllTags");
        }

        /// <inheritdoc />
        public string GetScriptAddTag()
        {
            return this.LoadSQLScript("AddTag");
        }

        /// <inheritdoc />
        public string GetScriptGetRolesOfUser()
        {
            return this.LoadSQLScript("GetRolesOfUser");
        }

        /// <inheritdoc />
        public string GetScriptGetStorageLocationIdOfContainee()
        {
            return this.LoadSQLScript("GetStorageLocationIdOfContainee");
        }

        /// <inheritdoc />
        public string GetScriptGetContentOfContainer()
        {
            return this.LoadSQLScript("GetContentOfContainer");
        }

        /// <inheritdoc />
        public string GetScriptDeleteDirectlyInheritedRoles()
        {
            return this.LoadSQLScript("DeleteDirectlyInheritedRoles");
        }

        /// <inheritdoc />
        public string GetScriptAddDirectlyInheritedRoles()
        {
            return this.LoadSQLScript("AddDirectlyInheritedRoles");
        }

        /// <inheritdoc />
        public string GetScriptInsertRole()
        {
            return this.LoadSQLScript("InsertRole");
        }

        /// <inheritdoc />
        public string GetScriptGetDirectlyInheritedRoleIds()
        {
            return this.LoadSQLScript("GetDirectlyInheritedRoleIds");
        }

        /// <inheritdoc />
        public string GetScriptGetUserByExternalLogin()
        {
            return this.LoadSQLScript("GetUserByExternalLogin");
        }

        /// <inheritdoc />
        public string GetScriptRenameFolder()
        {
            return this.LoadSQLScript("RenameFolder");
        }

        /// <inheritdoc />
        public string GetScriptRenameStorageLocation()
        {
            return this.LoadSQLScript("RenameStorageLocation");
        }
    }
}
