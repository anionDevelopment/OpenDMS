using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using OpenDMSBackend.Core.Database;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Misc;
using System.Linq;
using System.Data;
using Role = GRYLibrary.Core.APIServer.CommonDBTypes.Role;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.Misc.Strings;
using System.Data.Common;

namespace OpenDMSBackend.Core.Services.Misc
{
    public abstract class GenericPersistence : IPersistence
    {
        private readonly ISQLProvider _SQLProvider;
        private readonly DatabaseContext _DatabaseContext;
        private static readonly object _Lock = new object();
        private readonly Semaphore _Semaphore = new Semaphore();
        private readonly ITimeService _TimeService;
        private readonly IGRYLog _Log;
        public GenericPersistence(DbContextOptions<DatabaseContext> options, IGeneralLogger logger, ITimeService timeService, IDatabaseManager databaseManager, IGRYLog log, ISQLProvider sqlProvider)
        {
            this._TimeService = timeService;
            this._DatabaseContext = new DatabaseContext(options, logger, timeService, databaseManager);
            this._Log = log;
            this._SQLProvider = sqlProvider;
        }
        public abstract DbParameter GetParameter(string parameterName, object? value, Type type);
        public DbParameter GetParameter(string parameterName, object value)
        {
            GUtilities.AssertCondition(value != null, $"value for parameter {parameterName} is null, so a speicfic type for it must be set.");
            return this.GetParameter(parameterName, value, value.GetType());
        }


        #region AccessDatabase
        protected void AccessDatabase(Action<DatabaseContext> action)
        {
            this.AccessDatabase<object?>((database) =>
            {
                action(database);
                return null;
            });
        }

        protected T AccessDatabase<T>(Func<DatabaseContext, T> function)
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
        protected void RunTransaction(params Action<DbCommand>[] actions)
        {
            this.RunTransaction(actions.Select<Action<DbCommand>, Func<DbCommand, object?>>(action => (command) =>
            {
                action(command);
                return null;
            }
            ).ToArray());
        }

        protected T?[] RunTransaction<T>(params Func<DbCommand, T?>[] functions)
        {
            List<T?> results = new List<T?>();
            this.AccessDatabase(context =>
           {
               DbConnection connection = context.Connection;
               using DbTransaction transaction = connection.BeginTransaction();
               bool commit = true;
               try
               {
                   foreach (Func<DbCommand, T?> function in functions)
                   {
                       using (DbCommand cmd = connection.CreateCommand())
                       {
                           cmd.CommandType = CommandType.Text;
                           cmd.CommandTimeout = 300;
                           cmd.Transaction = transaction;
                           try
                           {
                               T? result = function(cmd);
                               results.Add(result);
                           }
                           catch
                           {
                               commit = false;
                               throw;
                           }
                       }
                       ;
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
        public virtual void CreateDocument(Document document)
        {
            this.RunTransaction((command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddDocument();

                command.Parameters.Add(this.GetParameter("Id", document.Id));
                command.Parameters.Add(this.GetParameter("Title", document.Title.Value));
                command.Parameters.Add(this.GetParameter("Filename", document.Filename.Value));
                command.Parameters.Add(this.GetParameter("OriginalFilename", document.OriginalFilename.Value));
                command.Parameters.Add(this.GetParameter("ImportDate", document.ImportDate.ToDateTime()));
                command.Parameters.Add(this.GetParameter("LastEditDate", document.LastEditDate.HasValue ? document.LastEditDate.Value.ToDateTime() : null, typeof(DateTime)));
                command.Parameters.Add(this.GetParameter("ReadableId", document.ReadableId));
                command.Parameters.Add(this.GetParameter("MIMEType", document.MIMEType.Value));
                command.Parameters.Add(this.GetParameter("DocumentContent", document.Content));
                command.Parameters.Add(this.GetParameter("OCRContent", document.OCRContent));
                command.Parameters.Add(this.GetParameter("DocumentPreview", document.Preview));
                command.ExecuteNonQuery();
            });
        }

        public virtual IDictionary<string, User> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public virtual ISet<Role> GetAllRoles()
        {
            ISet<Role> roles = GUtilities.GetValue(this.RunTransaction((command) =>
            {
                ISet<Role> rolesInternal = new HashSet<Role>();
                command.CommandText = this._SQLProvider.GetScriptGetAllRoles();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                        string name = reader.GetString(1);
                        rolesInternal.Add(new Role() { Id = id, Name = name });
                    }
                    reader.Close();
                    return rolesInternal;
                }
                ;
            })[0]);
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

        public virtual void AddRole(Role role)
        {
            this.RunTransaction((command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptInsertRole();

                command.Parameters.Add(this.GetParameter("Id", role.Id));
                command.Parameters.Add(this.GetParameter("Name", role.Name));
                command.ExecuteNonQuery();
            }, (command) =>
            {
                //TODO add inherited roles
            });
        }

        public virtual void UpdateRole(Role role)
        {
            this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUpdateRole();
                cmd.Parameters.Add(this.GetParameter("Id", role.Id));
                cmd.Parameters.Add(this.GetParameter("Name", role.Name));
                using DbDataReader reader = cmd.ExecuteReader();
            }, (cmd) =>
            {
                //TODO update inherited roles
            });
        }

        public virtual void DeleteRoleByName(string roleName)
        {
            throw new NotImplementedException();
        }

        public virtual bool AccessTokenExists(string accessToken, out User user)
        {
            throw new NotImplementedException();
        }

        public virtual void AddUser(User user)
        {
            this.RunTransaction((command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddUser();
                command.Parameters.Add(this.GetParameter("Id", user.Id));
                command.Parameters.Add(this.GetParameter("Name", user.Name));
                command.Parameters.Add(this.GetParameter("PasswordHash", user.PasswordHash, typeof(string)));
                command.Parameters.Add(this.GetParameter("EMailAddress", user.EMailAddress, typeof(string)));
                command.Parameters.Add(this.GetParameter("UserIsActivated", user.UserIsActivated));
                command.Parameters.Add(this.GetParameter("UserIsLocked", user.UserIsLocked));
                command.Parameters.Add(this.GetParameter("RegistrationMoment", user.RegistrationMoment));
                command.Parameters.Add(this.GetParameter("TOTPActivated", user.TOTP == null ? null : user.TOTP.IsActicated, typeof(bool)));
                command.Parameters.Add(this.GetParameter("TOTPSecretKey", user.TOTP == null ? null : user.TOTP.SecretKey, typeof(string)));

                command.ExecuteNonQuery();
            });
        }

        public virtual bool UserWithNameExists(string userName)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserWithNameExists();
                cmd.Parameters.Add(this.GetParameter("UserName", userName));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        public virtual bool UserWithIdExists(string userId)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserWithIdExists();
                cmd.Parameters.Add(this.GetParameter(nameof(userId), userId));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        public virtual User GetUserById(string userId)
        {
            User result = GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetUserById();
                cmd.Parameters.Add(this.GetParameter("Id", userId));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    User user = new User();
                    user.Id = userId;
                    user.Name = reader.GetString(1);
                    user.PasswordHash = reader.GetString(2);
                    user.EMailAddress = this.ConvertValue<string>(reader["EMailAddress"]);
                    user.UserIsActivated = reader.GetBoolean(4);
                    user.UserIsLocked = reader.GetBoolean(5);
                    user.RegistrationMoment = reader.GetDateTime(6);
                    return user;
                }
                else
                {
                    throw new KeyNotFoundException($"No user found with id '{userId}'");
                }
            })[0]);
            this.EnrichWhichAccessToken(result);
            this.EnrichWhichTOTPToken(result);
            return result;
        }

        public virtual User GetUserByName(string userName)
        {
            User result = GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetUserByName();
                cmd.Parameters.Add(this.GetParameter("Name", userName));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    User user = new User();
                    user.Id = reader.GetString(0);
                    user.Name = reader.GetString(1);
                    user.PasswordHash = reader.GetString(2);
                    user.EMailAddress = this.ConvertValue<string>(reader["EMailAddress"]);
                    user.UserIsActivated = reader.GetBoolean(4);
                    user.UserIsLocked = reader.GetBoolean(5);
                    user.RegistrationMoment = reader.GetDateTime(6);
                    return user;
                }
                else
                {
                    throw new KeyNotFoundException($"No user found with username '{userName}'");
                }
            })[0]);
            this.EnrichWhichAccessToken(result);
            this.EnrichWhichTOTPToken(result);
            return result;
        }

        private void EnrichWhichTOTPToken(User result)
        {
            //TODO
        }

        private void EnrichWhichAccessToken(User user)
        {
            this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetAllAccessTokenForUser();
                cmd.Parameters.Add(this.GetParameter("UserId", user.Id));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        user.AccessToken.Add(new AccessToken()
                        {
                            Value = reader.GetString(0),
                            ExpiredMoment = reader.GetDateTime(1),
                            OwnerUserId = user.Id
                        });
                    }
                }
            });
        }

        public virtual void RemoveUser(string userId)
        {
            throw new NotImplementedException();
        }

        public virtual bool RoleExists(string roleName)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptRoleExists();
                cmd.Parameters.Add(this.GetParameter("RoleName", roleName));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        public virtual void AddRoleToUser(string userId, string roleId)
        {
            this.RunTransaction((command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddRoleToUser();

                command.Parameters.Add(this.GetParameter("UserId", userId));
                command.Parameters.Add(this.GetParameter("RoleId", roleId));
                command.ExecuteNonQuery();
            });
        }

        public virtual void RemoveRoleFromUser(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public virtual bool UserHasRole(string userId, string roleId)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserHasRole();
                cmd.Parameters.Add(this.GetParameter("UserId", userId));
                cmd.Parameters.Add(this.GetParameter("RoleId", roleId));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        public virtual User GetUserByAccessToken(string accessToken)
        {
            return this.GetUserById(this.GetAccessToken(accessToken).OwnerUserId);
        }

        public virtual void UpdateUser(User user)
        {
            throw new NotImplementedException();
        }

        public virtual uint GetAmountOfDocuments()
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetAmountOfDocuments();
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return (uint)reader.GetInt32(0);
                }
                else
                {
                    return (uint)0;
                }
            })[0];
        }

        public virtual void Reset()
        {
            this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptResetDatabase();
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            });
        }

        public virtual bool IsAvailable()
        {
            return true;//TODO implement correctly
        }

        public virtual void Dispose()
        {
            if (this._DatabaseContext != null && this._DatabaseContext.Connection != null)
            {
                this._DatabaseContext.Connection.Close();
                this._DatabaseContext.Connection.Dispose();
            }
            this._DatabaseContext?.Dispose();
        }

        public virtual Document GetDocument(string id)
        {
            return GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                try
                {
                    cmd.CommandText = this._SQLProvider.GetScriptGetDocument();
                    cmd.Parameters.Add(this.GetParameter("Id", id));
                    using DbDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        GRYDateTime importDate = this.GetValue<GRYDateTime>(reader, 3, false);
                        GRYDateTime? lastEditDate = this.GetValue<GRYDateTime>(reader, 4, true);
                        Document document = new Document(id, OneLineString.From(reader.GetString(0)), OneLineString.From(reader.GetString(1)), OneLineString.From(reader.GetString(2)), importDate, lastEditDate, (uint)reader.GetInt32(5), new HashSet<Tag>(), OneLineString.From(reader.GetString(6)), (byte[])reader.GetValue(7), reader.GetString(8), (byte[])reader.GetValue(9));
                        //TODO load tags
                        return document;
                    }
                    else
                    {
                        throw new KeyNotFoundException($"No document found with document '{id}'");
                    }
                }
                catch
                {
                    throw;
                }
            })[0]);
        }

        public virtual void CreateTag(Tag tag)
        {
            throw new NotImplementedException();
        }

        public virtual void AssignTag(string documentId, string tagId)
        {
            throw new NotImplementedException();
        }

        public virtual void UnassignTag(string documentId, string tagId)
        {
            throw new NotImplementedException();
        }

        public virtual TagDTO[] GetAllTags()
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<string> GetAllDocumentIds()
        {
            throw new NotImplementedException();
        }

        public virtual string GetIdOfStorageLocationContainedIn(string containeeId)
        {
            throw new NotImplementedException();
        }

        public virtual bool UserIsOwnerOfStorageLocation(string userId, string storageLocationId)
        {
            throw new NotImplementedException();
        }

        public virtual bool StorageLocationIsSharedWithUser(string storageLocationId, string userId)
        {
            throw new NotImplementedException();
        }

        public virtual string AddStoragLocation(string name)
        {
            throw new NotImplementedException();
        }

        public virtual void SetOwnerOfStorageLocation(string storageLocationId, string userId)
        {
            throw new NotImplementedException();
        }

        public virtual string AddFolder(string name)
        {
            throw new NotImplementedException();
        }

        public virtual void SetParentOfContainee(IContainee containee, string parentContainerId)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(string containerOrContaineeId)
        {
            throw new NotImplementedException();
        }

        public virtual void AuthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId)
        {
            throw new NotImplementedException();
        }

        public virtual void UnauthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId)
        {
            throw new NotImplementedException();
        }

        public virtual void Rename(string containerId, string newName)
        {
            throw new NotImplementedException();
        }

        public virtual void Update(string requesterUserId, Document updatedDocument)
        {
            throw new NotImplementedException();
        }

        public virtual IContainer GetContainerById(string containerId)
        {
            if (this.IsStorageLocation(containerId))
            {
                return this.GetStorageLocation(containerId);
            }
            if (this.IsFolder(containerId))
            {
                return this.GetFolder(containerId);
            }
            throw new KeyNotFoundException($"No {nameof(IContainer)} available with id \"{containerId}\".");
        }

        public virtual IContainee GetContaineeById(string containeeId)
        {
            if (this.IsDocument(containeeId))
            {
                return this.GetDocument(containeeId);
            }
            if (this.IsFolder(containeeId))
            {
                return this.GetFolder(containeeId);
            }
            throw new KeyNotFoundException($"No {nameof(IContainee)} available with id \"{containeeId}\".");
        }

        public virtual string GetParentIdOfContainee(string containeeId)
        {
            return GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetParentIdOfContainee();
                cmd.Parameters.Add(this.GetParameter("Id", containeeId));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    string result = reader.GetString(0);
                    return result;
                }
                else
                {
                    throw new KeyNotFoundException($"No container found for containee '{containeeId}'.");
                }
            })[0]);
        }

        public virtual bool IsContaineeId(string id)
        {
            return this.IsDocument(id) || this.IsFolder(id);
        }

        public virtual bool IsStorageLocationId(string id)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsStorageLocation();
                cmd.Parameters.Add(this.GetParameter("ContentId", id));
                using DbDataReader reader = cmd.ExecuteReader();
                reader.Read();
                if (reader.HasRows)
                {
                    return reader.GetInt32(0) == 1;
                }
                else
                {
                    return false;
                }
            })[0];
        }

        public virtual DocumentPreview GetDocumentPreview(string id)
        {
            return GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetDocument();
                cmd.Parameters.Add(this.GetParameter("Id", id));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    DocumentPreview document = new DocumentPreview(id, OneLineString.From(reader.GetString(0)), OneLineString.From(reader.GetString(1)), OneLineString.From(reader.GetString(2)), GRYDateTime.FromDateTime(reader.GetDateTime(3)), GRYDateTime.FromDateTime(this.ConvertValue<DateTime>(reader.GetDateTime(4))), (uint)reader.GetInt32(5), new HashSet<Tag>(), OneLineString.From(reader.GetString(6)), (byte[])reader.GetValue(7));
                    //TODO load tags
                    return document;
                }
                else
                {
                    throw new KeyNotFoundException($"No document found with document '{id}'");
                }
            })[0]);
        }

        public virtual IEnumerable<string> GetAllStorageLocationIds()
        {
            ISet<string> result = GUtilities.GetValue(this.RunTransaction((command) =>
            {
                ISet<string> resultInternal = new HashSet<string>();
                command.CommandText = this._SQLProvider.GetScriptGetAllStorageLocations();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                        resultInternal.Add(id);
                    }
                    reader.Close();
                    return resultInternal;
                }
                ;
            })[0]);
            return result;
        }

        public virtual StorageLocation GetStorageLocation(string storageLocationId)
        {
            return GUtilities.GetValue(this.RunTransaction((command) =>
            {
                StorageLocation? result = null;
                command.CommandText = this._SQLProvider.GetScriptGetStorageLocation();
                command.Parameters.Add(this.GetParameter("Id", storageLocationId));
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = new StorageLocation()
                        {
                            Id = storageLocationId,
                            Name = reader.GetString(0),
                        };
                    }
                    reader.Close();
                    if (result == null)
                    {
                        throw new KeyNotFoundException($"No storage location found with id '{storageLocationId}'");
                    }
                    return result;
                }
                ;
            })[0]);
        }

        public virtual Folder GetFolder(string folderId)
        {
            return GUtilities.GetValue(this.RunTransaction((command) =>
            {
                Folder? result = null;
                command.CommandText = this._SQLProvider.GetScriptGetFolder();
                command.Parameters.Add(this.GetParameter("Id", folderId));
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = new Folder()
                        {
                            Id = folderId,
                            Name = reader.GetString(0),
                        };
                    }
                    reader.Close();
                    if (result == null)
                    {
                        throw new KeyNotFoundException($"No folder found with id '{folderId}'");
                    }
                    return result;
                }
                ;
            })[0]);
        }

        public virtual bool IsStorageLocation(string contentId)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsStorageLocation();
                cmd.Parameters.Add(this.GetParameter("ContentId", contentId));
                using DbDataReader reader = cmd.ExecuteReader();
                reader.Read();
                if (reader.HasRows)
                {
                    return reader.GetInt32(0) == 1;
                }
                else
                {
                    return false;
                }
            })[0];
        }

        public virtual bool IsFolder(string contentId)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsFolder();
                cmd.Parameters.Add(this.GetParameter("ContentId", contentId));
                using DbDataReader reader = cmd.ExecuteReader();
                reader.Read();
                if (reader.HasRows)
                {
                    return reader.GetInt32(0) == 1;
                }
                else
                {
                    return false;
                }
            })[0];
        }

        public virtual bool IsDocument(string contentId)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsDocument();
                cmd.Parameters.Add(this.GetParameter("ContentId", contentId));
                using DbDataReader reader = cmd.ExecuteReader();
                reader.Read();
                if (reader.HasRows)
                {
                    return reader.GetInt32(0) == 1;
                }
                else
                {
                    return false;
                }
            })[0];
        }

        public virtual AccessToken GetAccessToken(string accessToken)
        {
            return GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetAccessToken();
                cmd.Parameters.Add(this.GetParameter("Value", accessToken));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    AccessToken result = new AccessToken();
                    result.Value = accessToken;
                    result.ExpiredMoment = reader.GetDateTime(1);
                    result.OwnerUserId = reader.GetString(2);
                    return result;
                }
                else
                {
                    throw new KeyNotFoundException($"No access-token found with value '{accessToken}'");
                }
            })[0]);
        }

        public virtual void AddAccessToken(string userId, AccessToken newAccessToken)
        {
            this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptAddAccessToken();
                cmd.Prepare();
                cmd.Parameters.Add(this.GetParameter("Value", newAccessToken.Value));
                cmd.Parameters.Add(this.GetParameter("ExpiredMoment", newAccessToken.ExpiredMoment));
                cmd.Parameters.Add(this.GetParameter("UserId", userId));
                cmd.ExecuteNonQuery();
            });
        }

        public virtual void RemoveAccessToken(string accessToken)
        {
            this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptRemoveAccessToken();
                cmd.Parameters.Add(this.GetParameter("Value", accessToken));
                cmd.ExecuteNonQuery();
            });
        }

        public virtual void Housekeeping()
        {
            //TODO 
        }

        public virtual ulong GetLatestReadableId()
        {
            return this.GetAmountOfDocuments();
        }

        public virtual ISet<AccessToken> GetAllAccessTokenOfUser(string userId)
        {
            throw new NotImplementedException();
        }

        public virtual void RemoveChild(string parentId, string childId)
        {
            throw new NotImplementedException();
        }

        public virtual string GetIdFromReadableId(uint readableId)
        {
            return GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetIdFromReadableId();
                cmd.Parameters.Add(this.GetParameter("ReadableId", readableId));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    string result = reader.GetString(0);
                    return result;
                }
                else
                {
                    throw new KeyNotFoundException($"No document found for containee '{readableId}'.");
                }
            })[0]);
        }

        public virtual IList<string> Search(string searchTerm)
        {
            return GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                IDictionary<string, uint> result = new Dictionary<string, uint>();
                cmd.CommandText = this._SQLProvider.GetScriptSearch();
                cmd.Parameters.Add(this.GetParameter("SearchTerm", searchTerm));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        uint score = (uint)reader.GetInt32(1);
                        if (0 < score)
                        {
                            result[reader.GetString(0)] = score;
                        }
                    }
                }
                return result.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
            })[0]);
        }

        public virtual Role GetRoleByName(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}
