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
using MySqlConnector;
using Role = GRYLibrary.Core.APIServer.CommonDBTypes.Role;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using GRYLibrary.Core.Logging.GRYLogger;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.Misc.Strings;

namespace OpenDMSBackend.Core.Services
{
    public sealed class DatabasePersistence : IPersistence
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
            this.AccessDatabase<object?>((database) =>
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
            this.RunTransaction(actions.Select<Action<MySqlCommand>, Func<MySqlCommand, object?>>(action => (command) =>
            {
                action(command);
                return null;
            }
            ).ToArray());
        }

        public T?[] RunTransaction<T>(params Func<MySqlCommand, T?>[] functions)
        {
            List<T?> results = new List<T?>();
            this.AccessDatabase(context =>
           {
               MySqlConnection connection = context.Connection;
               using MySqlTransaction transaction = connection.BeginTransaction();
               bool commit = true;
               try
               {
                   foreach (Func<MySqlCommand, T?> function in functions)
                   {
                       using (MySqlCommand cmd = connection.CreateCommand())
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
                command.Parameters.Add(new MySqlParameter("MIMEType", document.MIMEType.Value));
                command.Parameters.Add(new MySqlParameter("DocumentContent", document.Content));
                command.Parameters.Add(new MySqlParameter("OCRContent", document.OCRContent));
                command.Parameters.Add(new MySqlParameter("DocumentPreview", document.Preview));
                command.ExecuteNonQuery();
            });
        }

        public IDictionary<string, User> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public ISet<Role> GetAllRoles()
        {
            ISet<Role> roles = GUtilities.GetValue(this.RunTransaction((command) =>
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

        public bool AccessTokenExists(string accessToken, out User user)
        {
            throw new NotImplementedException();
        }

        public void AddUser(User user)
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
                cmd.CommandText = this._SQLProvider.GetScriptUserWithIdExists();
                cmd.Parameters.Add(new MySqlParameter(nameof(userId), userId));
                using MySqlDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        public User GetUserById(string userId)
        {
            User result = GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetUserById();
                cmd.Parameters.Add(new MySqlParameter("Id", userId));
                using MySqlDataReader reader = cmd.ExecuteReader();
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

        public User GetUserByName(string userName)
        {
            User result = GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetUserByName();
                cmd.Parameters.Add(new MySqlParameter("Name", userName));
                using MySqlDataReader reader = cmd.ExecuteReader();
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
                cmd.Parameters.Add(new MySqlParameter("UserId", user.Id));
                using MySqlDataReader reader = cmd.ExecuteReader();
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

        private T? ConvertValue<T>(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return default(T);
            }
            else
            {
                return (T)value;
            }
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

        public User GetUserByAccessToken(string accessToken)
        {
            return this.GetUserById(this.GetAccessToken(accessToken).OwnerUserId);
        }

        public void UpdateUser(User user)
        {
            throw new NotImplementedException();
        }

        public uint GetAmountOfDocuments()
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetAmountOfDocuments();
                using MySqlDataReader reader = cmd.ExecuteReader();
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
            return true;//TODO implement correctly
        }

        public void Dispose()
        {
            this._DatabaseContext?.Dispose();
        }

        public Document GetDocument(string id)
        {
            return GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetDocument();
                cmd.Parameters.Add(new MySqlParameter("Id", id));
                using MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    Document document = new Document(id, OneLineString.From(reader.GetString(0)), OneLineString.From(reader.GetString(1)), OneLineString.From(reader.GetString(2)), GRYDateTime.FromDateTime(reader.GetDateTime(3)), GRYDateTime.FromDateTime(this.ConvertValue<DateTime>(reader.GetDateTime(4))), reader.GetUInt32(5), new HashSet<Tag>(), OneLineString.From(reader.GetString(6)), (byte[])reader.GetValue(7), reader.GetString(8), (byte[])reader.GetValue(9));
                    //TODO load tags
                    return document;
                }
                else
                {
                    throw new KeyNotFoundException($"No document found with document '{id}'");
                }
            })[0]);
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

        public TagDTO[] GetAllTags()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAllDocumentIds()
        {
            throw new NotImplementedException();
        }

        public string GetIdOfStorageLocationContainedIn(string containeeId)
        {
            throw new NotImplementedException();
        }

        public bool UserIsOwnerOfStorageLocation(string userId, string storageLocationId)
        {
            throw new NotImplementedException();
        }

        public bool StorageLocationIsSharedWithUser(string storageLocationId, string userId)
        {
            throw new NotImplementedException();
        }

        public string AddStoragLocation(string name)
        {
            throw new NotImplementedException();
        }

        public void SetOwnerOfStorageLocation(string storageLocationId, string userId)
        {
            throw new NotImplementedException();
        }

        public string AddFolder(string name)
        {
            throw new NotImplementedException();
        }

        public void SetParentOfContainee(IContainee containee, string parentContainerId)
        {
            throw new NotImplementedException();
        }

        public void Delete(string containerOrContaineeId)
        {
            throw new NotImplementedException();
        }

        public void AuthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId)
        {
            throw new NotImplementedException();
        }

        public void UnauthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId)
        {
            throw new NotImplementedException();
        }

        public void Rename(string containerId, string newName)
        {
            throw new NotImplementedException();
        }

        public void Update(string requesterUserId, Document updatedDocument)
        {
            throw new NotImplementedException();
        }

        public IContainer GetContainerById(string containerId)
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

        public IContainee GetContaineeById(string containeeId)
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

        public string GetParentIdOfContainee(string containeeId)
        {
            return GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetParentIdOfContainee();
                cmd.Parameters.Add(new MySqlParameter("Id", containeeId));
                using MySqlDataReader reader = cmd.ExecuteReader();
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

        public bool IsContaineeId(string id)
        {
            return this.IsDocument(id) || this.IsFolder(id);
        }

        public bool IsStorageLocationId(string id)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsStorageLocation();
                cmd.Parameters.Add(new MySqlParameter("ContentId", id));
                using MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                if (reader.HasRows)
                {
                    return reader.GetUInt32(0) == 1;
                }
                else
                {
                    return false;
                }
            })[0];
        }

        public DocumentPreview GetDocumentPreview(string id)
        {
            return GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetDocument();
                cmd.Parameters.Add(new MySqlParameter("Id", id));
                using MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    DocumentPreview document = new DocumentPreview(id, OneLineString.From(reader.GetString(0)), OneLineString.From(reader.GetString(1)), OneLineString.From(reader.GetString(2)), GRYDateTime.FromDateTime(reader.GetDateTime(3)), GRYDateTime.FromDateTime(this.ConvertValue<DateTime>(reader.GetDateTime(4))), reader.GetUInt32(5), new HashSet<Tag>(), OneLineString.From(reader.GetString(6)), (byte[])reader.GetValue(7));
                    //TODO load tags
                    return document;
                }
                else
                {
                    throw new KeyNotFoundException($"No document found with document '{id}'");
                }
            })[0]);
        }

        public IEnumerable<string> GetAllStorageLocationIds()
        {
            ISet<string> result = GUtilities.GetValue(this.RunTransaction((command) =>
            {
                ISet<string> resultInternal = new HashSet<string>();
                command.CommandText = this._SQLProvider.GetScriptGetAllStorageLocations();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                        resultInternal.Add(id);
                    }
                    reader.Close();
                    return resultInternal;
                };
            })[0]);
            return result;
        }

        public StorageLocation GetStorageLocation(string storageLocationId)
        {
            return GUtilities.GetValue(this.RunTransaction((command) =>
            {
                StorageLocation? result = null;
                command.CommandText = this._SQLProvider.GetScriptGetStorageLocation();
                command.Parameters.Add(new MySqlParameter("Id", storageLocationId));
                using (MySqlDataReader reader = command.ExecuteReader())
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
                };
            })[0]);
        }

        public Folder GetFolder(string folderId)
        {
            return GUtilities.GetValue(this.RunTransaction((command) =>
            {
                Folder? result = null;
                command.CommandText = this._SQLProvider.GetScriptGetFolder();
                command.Parameters.Add(new MySqlParameter("Id", folderId));
                using (MySqlDataReader reader = command.ExecuteReader())
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
                };
            })[0]);
        }

        public bool IsStorageLocation(string contentId)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsStorageLocation();
                cmd.Parameters.Add(new MySqlParameter("ContentId", contentId));
                using MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                if (reader.HasRows)
                {
                    return reader.GetUInt32(0) == 1;
                }
                else
                {
                    return false;
                }
            })[0];
        }

        public bool IsFolder(string contentId)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsFolder();
                cmd.Parameters.Add(new MySqlParameter("ContentId", contentId));
                using MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                if (reader.HasRows)
                {
                    return reader.GetUInt32(0) == 1;
                }
                else
                {
                    return false;
                }
            })[0];
        }

        public bool IsDocument(string contentId)
        {
            return this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsDocument();
                cmd.Parameters.Add(new MySqlParameter("ContentId", contentId));
                using MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                if (reader.HasRows)
                {
                    return reader.GetUInt32(0) == 1;
                }
                else
                {
                    return false;
                }
            })[0];
        }

        public AccessToken GetAccessToken(string accessToken)
        {
            return GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetAccessToken();
                cmd.Parameters.Add(new MySqlParameter("Value", accessToken));
                using MySqlDataReader reader = cmd.ExecuteReader();
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

        public void AddAccessToken(string userId, AccessToken newAccessToken)
        {
            this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptAddAccessToken();
                cmd.Prepare();
                cmd.Parameters.Add(new MySqlParameter("Value", newAccessToken.Value));
                cmd.Parameters.Add(new MySqlParameter("ExpiredMoment", newAccessToken.ExpiredMoment));
                cmd.Parameters.Add(new MySqlParameter("UserId", userId));
                cmd.ExecuteNonQuery();
            });
        }

        public void RemoveAccessToken(string accessToken)
        {
            this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptRemoveAccessToken();
                cmd.Parameters.Add(new MySqlParameter("Value", accessToken));
                cmd.ExecuteNonQuery();
            });
        }

        public void Housekeeping()
        {
            //TODO 
        }

        public ulong GetLatestReadableId()
        {
            return this.GetAmountOfDocuments();
        }

        public ISet<AccessToken> GetAllAccessTokenOfUser(string userId)
        {
            throw new NotImplementedException();
        }

        public void RemoveChild(string parentId, string childId)
        {
            throw new NotImplementedException();
        }

        public string GetIdFromReadableId(uint readableId)
        {
            return GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetIdFromReadableId();
                cmd.Parameters.Add(new MySqlParameter("ReadableId", readableId));
                using MySqlDataReader reader = cmd.ExecuteReader();
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

        public IList<string> Search(string searchTerm)
        {
            return GUtilities.GetValue(this.RunTransaction((cmd) =>
            {
                IDictionary<string, uint> result = new Dictionary<string, uint>();
                cmd.CommandText = this._SQLProvider.GetScriptSearch();
                cmd.Parameters.Add(new MySqlParameter("SearchTerm", searchTerm));
                using MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        uint score = reader.GetUInt32(1);
                        if (0 < score)
                        {
                            result[reader.GetString(0)] = score;
                        }
                    }
                }
                return result.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
            })[0]);
        }
    }
}
