using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using OpenDMSBackend.Core.Model;
using OpenDMSBackend.Core.Database;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc;
using System.Linq;
using System.Data;
using MySqlConnector;
using Role = GRYLibrary.Core.APIServer.CommonDBTypes.Role;
using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Services;

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public sealed class DatabasePersistence : IPersistence, IAuthenticationServicePersistence<Model.User>
    {
        private readonly ISQLProvider _SQLProvider;
        private readonly DatabaseContext _DatabaseContext;
        private static readonly object _Lock = new object();
        private readonly Semaphore _Semaphore = new Semaphore();
        private readonly ITimeService _TimeService;
        private readonly IGRYLog _Log;

        public DatabasePersistence(DbContextOptions<DatabaseContext> options, IGeneralLogger logger, ITimeService timeService, IDatabaseManager databaseManager, IGRYLog log, ISQLProvider sqlProvider)
        {
            this._TimeService = timeService;
            this._DatabaseContext = new DatabaseContext(options, logger, timeService, databaseManager);
            this._Log = log;
            this._SQLProvider = sqlProvider;
        }

        #region AccessDatabase
        private void AccessDatabase(Action<DatabaseContext> action)
        {
            this.AccessDatabase<object>((database) =>
                {
                    action(database);
                    return null;
                });
        }

        private T AccessDatabase<T>(Func<DatabaseContext, T> function)
        {
            lock (_Lock)
            {
                this._Semaphore.Lock();
                try
                {
                    return function(this._DatabaseContext);
                }
                finally
                {
                    this._Semaphore.Unlock();
                }
            }
        }
        public void RunTransaction(params Action<MySqlCommand>[] actions)
        {
            this.RunTransaction(actions.Select<Action<MySqlCommand>, Func<MySqlCommand, object>>(action => (command) =>
                {
                    action(command);
                    return null;
                }
            ).ToArray());
        }

        public T[] RunTransaction<T>(params Func<MySqlCommand, T>[] functions)
        {
            List<T> results = new List<T>();
            this.AccessDatabase(context =>
           {
               MySqlConnection connection = context.Connection;
               using MySqlTransaction transaction = connection.BeginTransaction();
               bool commit = true;
               try
               {
                   foreach (Func<MySqlCommand, T> function in functions)
                   {
                       using (MySqlCommand cmd = connection.CreateCommand())
                       {
                           cmd.CommandType = CommandType.Text;
                           cmd.CommandTimeout = 300;
                           cmd.Transaction = transaction;
                           try
                           {
                               T result = function(cmd);
                               results.Add(result);
                           }
                           catch
                           {
                               commit = false;
                               throw;
                           }
                       };
                   }
               }
               finally
               {
                   if (commit)
                   {
                       transaction.Commit();
                   }
                   else
                   {
                       transaction.Rollback();
                   }
               }
           });
            return results.ToArray();
        }


        #endregion
        public void CreateDocument(Document document)
        {
            this.RunTransaction((command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddDocument();
                command.Prepare();
                command.Parameters.Add(new MySqlParameter("Id", document.Id));
                command.Parameters.Add(new MySqlParameter("Title", document.Title.Value));
                command.Parameters.Add(new MySqlParameter("Filename", document.Filename.Value));
                command.Parameters.Add(new MySqlParameter("OriginalFilename", document.OriginalFilename.Value));
                command.Parameters.Add(new MySqlParameter("ImportDate", document.ImportDate.ToDateTime()));
                command.Parameters.Add(new MySqlParameter("LastEditDate", document.LastEditDate.HasValue ? document.LastEditDate : null));
                command.Parameters.Add(new MySqlParameter("ReadableId", document.ReadableId));
                command.Parameters.Add(new MySqlParameter("DocumentContent", document.DocumentContent));
                command.Parameters.Add(new MySqlParameter("DocumentPreview", document.DocumentPreview));
                command.ExecuteNonQuery();
            });
        }

        public IDictionary<string, Model.User> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public ISet<Role> GetAllRoles()
        {
            ISet<Role> roles = this.RunTransaction((command) =>
            {
                ISet<Role> rolesInternal = new HashSet<Role>();
                command.CommandText = this._SQLProvider.GetScriptGetAllRoles();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                        string name = reader.GetString(1);
                        rolesInternal.Add(new Role() { Id = id, Name = name });
                    }
                    reader.Close();
                    return rolesInternal;
                };
            })[0];
            foreach (Role role in roles)
            {
                this.EnrichWithInheritedRoles(role);
            }
            return roles;
        }

        private void EnrichWithInheritedRoles(Role role)
        {
            //TODO load inherited roles transitively
        }

        public void AddRole(Role role)
        {
            this.RunTransaction((command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptInsertRole();
                command.Prepare();
                command.Parameters.Add(new MySqlParameter("Id", role.Id));
                command.Parameters.Add(new MySqlParameter("Name", role.Name));
                command.ExecuteNonQuery();
            }, (command) =>
            {
                //TODO add inherited roles
            });
        }

        public void UpdateRole(Role role)
        {
            this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUpdateRole();
                cmd.Parameters.Add(new MySqlParameter("Id", role.Id));
                cmd.Parameters.Add(new MySqlParameter("Name", role.Name));
                using MySqlDataReader reader = cmd.ExecuteReader();
            }, (cmd) =>
            {
                //TODO update inherited roles
            });
        }

        public void DeleteRoleByName(string roleName)
        {
            throw new NotImplementedException();
        }

        public bool AccessTokenExists(string accessToken, out Model.User user)
        {
            throw new NotImplementedException();
        }

        public void AddUser(Model.User user)
        {
            this.RunTransaction((command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddUser();
                command.Parameters.Add(new MySqlParameter("Id", user.Id));
                command.Parameters.Add(new MySqlParameter("Name", user.Name));
                command.Parameters.Add(new MySqlParameter("PasswordHash", user.PasswordHash));
                command.Parameters.Add(new MySqlParameter("EMailAddress", user.EMailAddress));
                command.Parameters.Add(new MySqlParameter("UserIsActivated", user.UserIsActivated));
                command.Parameters.Add(new MySqlParameter("UserIsLocked", user.UserIsLocked));
                command.Parameters.Add(new MySqlParameter("RegistrationMoment", user.RegistrationMoment));
                command.Parameters.Add(new MySqlParameter("TOTPActivated", user.TOTP.IsActicated));
                command.Parameters.Add(new MySqlParameter("TOTPSecretKey", user.TOTP.SecretKey));
                command.Prepare();
                command.ExecuteNonQuery();
            });
        }

        public bool UserWithNameExists(string userName)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserWithNameExists();
                cmd.Parameters.Add(new MySqlParameter("UserName", userName));
                using MySqlDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        public bool UserWithIdExists(string userId)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = _SQLProvider.GetScriptUserWithIdExists();
                cmd.Parameters.Add(new MySqlParameter(nameof(userId), userId));
                using MySqlDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        public Model.User GetUserById(string userId)
        {
            throw new NotImplementedException();
        }

        public Model.User GetUserByName(string userName)
        {
            throw new NotImplementedException();
        }

        public void RemoveUser(string userId)
        {
            throw new NotImplementedException();
        }

        public bool RoleExists(string roleName)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptRoleExists();
                cmd.Parameters.Add(new MySqlParameter("RoleName", roleName));
                using MySqlDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        public void AddRoleToUser(string userId, string roleId)
        {
            this.RunTransaction((command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddRoleToUser();
                command.Prepare();
                command.Parameters.Add(new MySqlParameter("UserId", userId));
                command.Parameters.Add(new MySqlParameter("RoleId", roleId));
                command.ExecuteNonQuery();
            });
        }

        public void RemoveRoleFromUser(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public bool UserHasRole(string userId, string roleId)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserHasRole();
                cmd.Parameters.Add(new MySqlParameter("UserId", userId));
                cmd.Parameters.Add(new MySqlParameter("RoleId", roleId));
                using MySqlDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        public Model.User GetUserByAccessToken(string accessToken)
        {
            throw new NotImplementedException();
        }

        public void UpdateUser(Model.User user)
        {
            throw new NotImplementedException();
        }

        public bool DocumentExists(string id)
        {
            throw new NotImplementedException();
        }

        public uint GetAmountOfDocuments()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptResetDatabase();
                using MySqlDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            });
        }

        public bool IsAvailable()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ulong GetNewReadableId()
        {
            throw new NotImplementedException();
        }

        public Document GetDocument(string id)
        {
            throw new NotImplementedException();
        }

        public void CreateTag(Tag tag)
        {
            throw new NotImplementedException();
        }

        public void AssignTag(string documentId, string tagId)
        {
            throw new NotImplementedException();
        }

        public void UnassignTag(string documentId, string tagId)
        {
            throw new NotImplementedException();
        }
    }
}
