using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Utilities;
using GRYLibrary.Core.APIServer.Utilities.InitializationStates;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.Strings;
using OpenDMSBackend.Core.Model.BusinessTypes;
using OpenDMSBackend.Core.Model.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using GUtilities = GRYLibrary.Core.Misc.Utilities;
using Role = GRYLibrary.Core.APIServer.CommonDBTypes.Role;

namespace OpenDMSBackend.Core.Services
{
    public sealed class DatabasePersistence : IInitializable, IPersistence
    {
        private readonly ISQLProvider _SQLProvider;
        private static readonly object _Lock = new object();
        private readonly ITimeService _TimeService;
        private readonly IGRYLog _Log;
        private readonly IOpenDMSDatabaseInteractor _Database;

        public InitializationState InitializationState { get; private set; }

        public DatabasePersistence(IOpenDMSDatabaseInteractor database, ITimeService timeService, IGRYLog log)
        {
            this._TimeService = timeService;
            this._Database = database;
            this._Log = log;
            this._SQLProvider = database.GetSQLProvider();
            this.InitializationState = new Uninitialized();
        }

        #region AccessDatabase
        protected void AccessDatabase(Action<IOpenDMSDatabaseInteractor> action)
        {
            this.AccessDatabase<object?>((database) =>
            {
                action(database);
                return null;
            });
        }

        protected T AccessDatabase<T>(Func<IOpenDMSDatabaseInteractor, T> function)
        {
            lock (_Lock)
            {
                return function(this._Database);
            }
        }
        protected void RunTransaction(string nameOfAction, params Action<DbCommand>[] actions)
        {
            this.RunTransaction(nameOfAction, actions.Select<Action<DbCommand>, Func<DbCommand, object?>>(action => (command) =>
            {
                action(command);
                return null;
            }
            ).ToArray());
        }

        protected T?[] RunTransaction<T>(string nameOfAction, params Func<DbCommand, T?>[] functions)
        {
            List<T?> results = new List<T?>();
            this.AccessDatabase(interactor =>
            {
                this._Log.Log("Run DB-transaction " + nameOfAction, Microsoft.Extensions.Logging.LogLevel.Trace);
                DbConnection connection = interactor.GetGenericDatabaseInteractor().GetConnection();
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
                            catch (Exception e)
                            {
                                commit = false;
                                this._Log.Log($"Error in database occurred while doing DB-transaction {nameOfAction}.", e);
                                throw;
                            }
                        }
                    }
                }
                finally
                {
                    if (commit)
                    {
                        this._Log.Log("Commit DB-transaction " + nameOfAction, Microsoft.Extensions.Logging.LogLevel.Trace);
                        transaction.Commit();
                    }
                    else
                    {
                        this._Log.Log("Rollback DB-transaction " + nameOfAction, Microsoft.Extensions.Logging.LogLevel.Trace);
                        transaction.Rollback();
                    }
                }
            });
            return results.ToArray();
        }

        #endregion
        public void CreateDocument(Document document)
        {
            this.RunTransaction(nameof(CreateDocument), (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddDocument();

                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", document.Id));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Title", document.Title.Value));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Filename", document.Filename.Value));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("OriginalFilename", document.OriginalFilename.Value));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ImportDate", this.ToDateTime(document.ImportDate), typeof(DateTime)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("LastEditDate", this.ToDateTime(document.LastEditDate), typeof(DateTime)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ReadableId", document.ReadableId));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("MIMEType", document.MIMEType.Value));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("DocumentContent", document.Content));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("OCRContent", document.OCRContent));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("DocumentPreview", document.Preview));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("IsSoftDeleted", document.IsSoftDeleted));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("DeleteIsNotAllowedBefore", this.ToDateTime(document.DeleteIsNotAllowedBefore), typeof(DateTime)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("MustBeHardDeletedAfter", this.ToDateTime(document.MustBeHardDeletedAfter), typeof(DateTime)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("GroupOfBusinessOwner", document.GroupOfBusinessOwner));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Version", document.Version.ToString()));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("AssignedLanguages", Core.Misc.Utilities.LanguagesListToString(document.AssignedLanguages)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("AddedByUserId", document.AddedByUserId, typeof(string)));
                command.ExecuteNonQuery();
            });
        }

        private DateTime? ToDateTime(DateTimeOffset? value)
        {
            if (value.HasValue && !default(DateTimeOffset).Equals(value))
            {
                // return value.Value.UtcDateTime;//does not work due to "    System.ArgumentException: Cannot write DateTime with Kind=UTC to PostgreSQL type 'timestamp without time zone', consider using 'timestamp with time zone'. Note that it's not possible to mix DateTimes with different Kinds in an array, range, or multirange. (Parameter 'value')
                DateTime dt = value.Value.UtcDateTime;
                return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }
            else
            {
                return null;
            }
        }
        private DateTimeOffset ToDateTimeOffset(DateTime dateTime)
        {
            try
            {
                return new DateTimeOffset(dateTime);
            }
            catch
            {
                throw;
            }
        }
        private DateTimeOffset? ToNullableDateTimeOffset(DateTime? dateTime)
        {
            if (dateTime.HasValue && !default(DateTime).Equals(dateTime))
            {
                return this.ToDateTimeOffset(dateTime.Value);
            }
            else
            {
                return default(DateTimeOffset);
            }
        }

        public IDictionary<string, User> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public ISet<Role> GetAllRoles()
        {
            lock (_Lock)
            {
                ISet<Role> roles = GUtilities.GetValue(this.RunTransaction(nameof(GetAllRoles), (command) =>
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
        }

        private void EnrichWithInheritedRoles(Role role)
        {
            role.InheritedRoles = new HashSet<Role>();

            foreach (string directlyInheritedRoleId in this.GetDirectlyInheritedRoleIds(role.Id))
            {
                Role inheritedRole = this.GetRoleById(directlyInheritedRoleId);
                role.InheritedRoles.Add(inheritedRole);
            }
        }



        private ISet<string> GetDirectlyInheritedRoleIds(string roleId)
        {
            ISet<string> roles = this.RunTransaction<ISet<string>>(nameof(EnrichWithInheritedRoles) + "_" + roleId, (cmd) =>
              {
                  ISet<string> directlyInheritedRoleIds = new HashSet<string>();
                  cmd.CommandText = this._SQLProvider.GetScriptGetInheritedRoles();
                  cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("RoleId", roleId));
                  using (DbDataReader reader = cmd.ExecuteReader())
                  {

                      if (reader.HasRows)
                      {
                          while (reader.Read())
                          {
                              directlyInheritedRoleIds.Add(reader.GetString(0));
                          }
                      }
                  }
                  return directlyInheritedRoleIds;
              })[0]!;//TODO check why esclamation-mark-operator is required here.
            return roles;
        }

        public void AddRole(Role role)
        {
            this.RunTransaction(nameof(AddRole), (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddRole();

                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", role.Id));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", role.Name));
                command.ExecuteNonQuery();
            }, (command) =>
            {
                //TODO add inherited roles
            });
        }

        public void UpdateRole(Role role)
        {
            List<Action<DbCommand>> actions = new List<Action<DbCommand>>();
            actions.Add((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUpdateRole();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", role.Id));
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", role.Name));
                cmd.ExecuteNonQuery();
            });
            actions.Add((cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptDeleteInheritedRoles();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("RoleId", role.Id));
                cmd.ExecuteNonQuery();
            });
            foreach (Role inheritedRole in role.InheritedRoles)
            {
                actions.Add((cmd) =>
                {
                    cmd.CommandText = this._SQLProvider.GetScriptAddInheritedRole();
                    cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("RoleId", role.Id));
                    cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("InheritedRoleId", inheritedRole.Id));
                    cmd.ExecuteNonQuery();
                });
            }
            this.RunTransaction(nameof(UpdateRole) + "_" + role.Id, actions.ToArray());
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
            this.RunTransaction(nameof(AddUser) + "_" + user.Id, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddUser();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", user.Id));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", user.Name));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("PasswordHash", user.PasswordHash, typeof(string)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("EMailAddress", user.EMailAddress, typeof(string)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserIsActivated", user.UserIsActivated));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserIsLocked", user.UserIsLocked));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("RegistrationMoment", user.RegistrationMoment));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("TOTPActivated", user.TOTP?.IsActicated, typeof(bool)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("TOTPSecretKey", user.TOTP?.SecretKey, typeof(string)));

                command.ExecuteNonQuery();
            });
        }

        public bool UserWithNameExists(string userName)
        {
            return this.RunTransaction(nameof(UserWithNameExists), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserWithNameExists();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserName", userName));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        public bool UserWithIdExists(string userId)
        {
            return this.RunTransaction(nameof(UserWithIdExists), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserWithIdExists();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter(nameof(userId), userId));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        public User GetUserById(string userId)
        {
            User result = GUtilities.GetValue(this.RunTransaction(nameof(GetUserById) + "_" + userId, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetUserById();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", userId));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    User user = new User();
                    user.Id = userId;
                    user.Name = reader.GetString(1);
                    user.PasswordHash = DBUtilities.GetNullableValue<string>(reader, 2);
                    user.EMailAddress = DBUtilities.GetNullableValue<string>(reader, 3);
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
            User result = GUtilities.GetValue(this.RunTransaction(nameof(GetUserByName) + "_" + userName, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetUserByName();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", userName));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    User user = new User();
                    user.Id = reader.GetString(0);
                    user.Name = reader.GetString(1);
                    user.PasswordHash = DBUtilities.GetNullableValue<string>(reader, 2);
                    user.EMailAddress = DBUtilities.GetNullableValue<string>(reader, 3);
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
            this.RunTransaction(nameof(EnrichWhichAccessToken), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetAllAccessTokenForUser();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserId", user.Id));
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

        public void RemoveUser(string userId)
        {
            throw new NotImplementedException();
        }

        public bool RoleExists(string roleName)
        {
            return this.RunTransaction(nameof(RoleExists), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptRoleExists();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("RoleName", roleName));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        public void AddRoleToUser(string userId, string roleId)
        {
            this.RunTransaction(nameof(AddRoleToUser), (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddRoleToUser();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserId", userId));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("RoleId", roleId));
                command.ExecuteNonQuery();
            });
        }

        public void RemoveRoleFromUser(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public bool UserHasRole(string userId, string roleId)
        {
            return this.RunTransaction(nameof(UserHasRole), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserHasRole();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserId", userId));
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("RoleId", roleId));
                using DbDataReader reader = cmd.ExecuteReader();
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
            return this.RunTransaction(nameof(GetAmountOfDocuments), (cmd) =>
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


        public (bool, Exception?) IsAvailable()
        {
            bool result = this._Database.GetGenericDatabaseInteractor().TryGetConnection(out _, out Exception? e);
            return (result, e);
        }

        public Document GetDocument(string id)
        {
            lock (_Lock)
            {
                Document result = GUtilities.GetValue(this.RunTransaction(nameof(GetDocument), (cmd) =>
                {
                    try
                    {
                        cmd.CommandText = this._SQLProvider.GetScriptGetDocument();
                        cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", id));
                        using DbDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            reader.Read();
                            Document document = new Document(id,
                                OneLineString.From(reader.GetString(0)),//title
                                OneLineString.From(reader.GetString(1)),//filename
                                OneLineString.From(reader.GetString(2)),//original filename
                                this.ToDateTimeOffset(DBUtilities.GetNullableValue<DateTime>(reader, 3)),//importdate
                                this.ToNullableDateTimeOffset(DBUtilities.GetNullableValue<DateTime>(reader, 4)),//lasteditdate
                                (uint)reader.GetInt32(5),//readableid
                                new HashSet<Tag>(),//tags
                                OneLineString.From(reader.GetString(6)),//mimetype
                                (byte[])reader.GetValue(7),//content
                                reader.GetString(8),//ocrcontent
                                (byte[])reader.GetValue(9),//preview
                                reader.GetBoolean(10),//is soft deleted
                                this.ToNullableDateTimeOffset(DBUtilities.GetNullableValue<DateTime>(reader, 11)),//delete is not allowed before
                                 this.ToNullableDateTimeOffset(DBUtilities.GetNullableValue<DateTime>(reader, 12)),//must be deleted after
                                reader.GetString(13),//businessowner
                                Version3.Parse(reader.GetString(14)),//version
                                Core.Misc.Utilities.StringToLanguagesList(reader.GetString(15)),//languages
                                reader.GetString(16)//userid
                            );
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
                this.EnrichWhichTags(result);
                return result;
            }
        }

        private void EnrichWhichTags(Document document)
        {
            ISet<string> tagIds = this.GetTagIdsOfDocument(document.Id);
            foreach (string tagId in tagIds)
            {
                document.Tags.Add(this.GetTag(tagId));
            }
        }

        public void CreateTag(Tag tag)
        {
            this.RunTransaction(nameof(CreateTag), (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddTag();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", tag.Id));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", tag.Name));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Color", tag.Color.ColorCode));
                command.ExecuteNonQuery();
            });
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
            HashSet<TagDTO> result = new HashSet<TagDTO>();
            this.RunTransaction(nameof(GetAllTags), (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptGetAllTags();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tag tag = new Tag(
                            reader.GetString(0),
                            reader.GetString(0),
                            new ExtendedColor(reader.GetInt32(2))
                        );
                    }
                }
            });
            return result.ToArray();
        }
        public Tag GetTag(string id)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetTag), (command) =>
              {
                  command.CommandText = this._SQLProvider.GetScriptGetTag();
                  command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", id));
                  using DbDataReader reader = command.ExecuteReader();
                  if (reader.HasRows)
                  {
                      reader.Read();
                      return new Tag(
                        reader.GetString(0),
                        reader.GetString(0),
                        new ExtendedColor(reader.GetInt32(2))
                      );
                  }
                  else
                  {
                      throw new KeyNotFoundException($"No tag found with id '{id}'.");
                  }
              })[0]);
        }
        public ISet<string> GetTagIdsOfDocument(string documentId)
        {
            HashSet<string> result = new HashSet<string>();
            this.RunTransaction(nameof(GetTagIdsOfDocument), (command) =>
             {
                 command.CommandText = this._SQLProvider.GetScriptGetTagsOfDocument();
                 command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("DocumentId", documentId));
                 using (DbDataReader reader = command.ExecuteReader())
                 {
                     while (reader.Read())
                     {
                         result.Add(reader.GetString(0));
                     }
                 }
             });
            return result;
        }

        public ISet<string> GetAllDocumentIds()
        {
            HashSet<string> result = new HashSet<string>();
            this.RunTransaction(nameof(GetAllDocumentIds), (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptGetAllDocumentIds();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }
            });
            return result;
        }

        public string GetIdOfStorageLocationContainedIn(string containeeId)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetIdOfStorageLocationContainedIn), (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptGetTag();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ContaineeId", containeeId));
                using DbDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return reader.GetString(0);
                }
                else
                {
                    throw new KeyNotFoundException($"No storage-location found for containee with id '{containeeId}'.");
                }
            })[0]);
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
            string id = Guid.NewGuid().ToString();
            this.RunTransaction(nameof(AddStoragLocation), (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddStorageLocation();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", id));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", name));
                command.ExecuteNonQuery();
            });
            return id;
        }

        public void SetOwnerOfStorageLocation(string storageLocationId, string userId)
        {
            this.RunTransaction(nameof(SetOwnerOfStorageLocation), (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddOrUpdateOwnerOfStorageLocation();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("StorageLocationId", storageLocationId));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserId", userId));
                command.ExecuteNonQuery();
            });
        }

        public string AddFolder(string name)
        {
            string id = Guid.NewGuid().ToString();
            this.RunTransaction(nameof(AddFolder), (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddFolder();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", id));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", name));
                command.ExecuteNonQuery();
            });
            return id;
        }

        public void SetParentOfContainee(IContainee containee, string parentContainerId)
        {
            this.RunTransaction(nameof(SetParentOfContainee), (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptSetParentOfContainee();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ContainerId", parentContainerId));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ContaineeId", containee.Id));
                command.ExecuteNonQuery();
            });
        }

        public void HardDelete(string containerOrContaineeId)
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
            return GUtilities.GetValue(this.RunTransaction(nameof(GetParentIdOfContainee), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetParentIdOfContainee();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", containeeId));
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

        public bool IsContaineeId(string id)
        {
            return this.IsDocument(id) || this.IsFolder(id);
        }

        public bool IsStorageLocationId(string id)
        {
            return this.RunTransaction(nameof(IsStorageLocationId), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsStorageLocation();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ContentId", id));
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

        public DocumentPreview GetDocumentPreview(string id)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetDocumentPreview), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetDocumentPreview();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", id));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    DocumentPreview document = new DocumentPreview(id, OneLineString.From(reader.GetString(0)), OneLineString.From(reader.GetString(1)), OneLineString.From(reader.GetString(2)), this.ToDateTimeOffset(reader.GetDateTime(3)), this.ToNullableDateTimeOffset(DBUtilities.GetNullableValue<DateTime>(reader, 4)), new HashSet<Tag>(), (uint)reader.GetInt32(5), OneLineString.From(reader.GetString(6)), (byte[])reader.GetValue(7), reader.GetBoolean(8), this.ToNullableDateTimeOffset(DBUtilities.GetNullableValue<DateTime>(reader, 9)), this.ToNullableDateTimeOffset(DBUtilities.GetNullableValue<DateTime>(reader, 10)), reader.GetString(11), Version3.Parse(reader.GetString(12)), Core.Misc.Utilities.StringToLanguagesList(GUtilities.GetValue(DBUtilities.GetNullableValue<string>(reader, 12))), reader.GetString(13));
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
            ISet<string> result = GUtilities.GetValue(this.RunTransaction(nameof(GetAllStorageLocationIds), (command) =>
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

        public StorageLocation GetStorageLocation(string storageLocationId)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetStorageLocation), (command) =>
            {
                StorageLocation? result = null;
                command.CommandText = this._SQLProvider.GetScriptGetStorageLocation();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", storageLocationId));
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

        public Folder GetFolder(string folderId)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetFolder), (command) =>
            {
                Folder? result = null;
                command.CommandText = this._SQLProvider.GetScriptGetFolder();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", folderId));
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

        public bool IsStorageLocation(string contentId)
        {
            return this.RunTransaction(nameof(IsStorageLocation), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsStorageLocation();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", contentId));
                using DbDataReader reader = cmd.ExecuteReader();
                reader.Read();
                return reader.HasRows;
            })[0];
        }

        public bool IsFolder(string contentId)
        {
            return this.RunTransaction(nameof(IsFolder), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsFolder();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", contentId));
                using DbDataReader reader = cmd.ExecuteReader();
                reader.Read();
                return reader.HasRows;
            })[0];
        }

        public bool IsDocument(string contentId)
        {
            return this.RunTransaction(nameof(IsDocument), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsDocument();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", contentId));
                using DbDataReader reader = cmd.ExecuteReader();
                reader.Read();
                return reader.HasRows;
            })[0];
        }

        public AccessToken GetAccessToken(string accessToken)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetAccessToken), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetAccessToken();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Value", accessToken));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    AccessToken result = new AccessToken();
                    result.Value = accessToken;
                    result.ExpiredMoment = reader.GetFieldValue<DateTimeOffset>(0);
                    result.OwnerUserId = reader.GetString(1);
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
            this.RunTransaction(nameof(AddAccessToken), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptAddAccessToken();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Value", newAccessToken.Value));
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ExpiredMoment", newAccessToken.ExpiredMoment));
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserId", userId));
                cmd.ExecuteNonQuery();
            });
        }

        public void RemoveAccessToken(string accessToken)
        {
            this.RunTransaction(nameof(RemoveAccessToken), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptRemoveAccessToken();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Value", accessToken));
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
            return GUtilities.GetValue(this.RunTransaction(nameof(Search), (cmd) =>
            {
                ISet<AccessToken> result = new HashSet<AccessToken>();
                cmd.CommandText = this._SQLProvider.GetScriptGetAllAccessTokenForUser();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserId", userId));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        AccessToken accessToken = new AccessToken();
                        accessToken.OwnerUserId = userId;
                        accessToken.Value = reader.GetString(0);
                        accessToken.ExpiredMoment = reader.GetDateTime(1);
                        result.Add(accessToken);
                    }
                }
                return result;
            })[0]);
        }

        public void RemoveChild(string parentId, string childId)
        {
            throw new NotImplementedException();
        }

        public string GetIdFromReadableId(uint readableId)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetIdFromReadableId), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetIdFromReadableId();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ReadableId", readableId));
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

        public IList<string> Search(string searchTerm)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(Search), (cmd) =>
            {
                IDictionary<string, uint> result = new Dictionary<string, uint>();
                cmd.CommandText = this._SQLProvider.GetScriptSearch();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("SearchTerm", searchTerm));
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

        public Role GetRoleByName(string roleName)
        {
            lock (_Lock)
            {
                Role result = GUtilities.GetValue(this.RunTransaction(nameof(GetRoleByName) + "_" + roleName, (cmd) =>
                {
                    cmd.CommandText = this._SQLProvider.GetScriptGetRoleByName();
                    cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", roleName));
                    using DbDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        Role role = new Role();
                        reader.Read();
                        role.Id = reader.GetString(0);
                        role.Name = roleName;
                        return role;
                    }
                    else
                    {
                        throw new KeyNotFoundException($"No role found with name '{roleName}'.");
                    }
                })[0]);
                this.EnrichWithInheritedRoles(result);
                return result;
            }
        }

        public Role GetRoleById(string roleId)
        {
            lock (_Lock)
            {
                Role result = GUtilities.GetValue(this.RunTransaction(nameof(GetRoleById) + "_" + roleId, (cmd) =>
                {
                    Role role = new Role();
                    cmd.CommandText = this._SQLProvider.GetScriptGetRoleById();
                    cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", roleId));
                    using (DbDataReader reader = cmd.ExecuteReader())
                    {

                        if (reader.HasRows)
                        {
                            reader.Read();
                            role.Id = roleId;
                            role.Name = reader.GetString(0);
                        }
                        else
                        {
                            throw new KeyNotFoundException($"No role found with id '{roleId}'.");
                        }
                    }
                    return role;
                })[0]);
                this.EnrichWithInheritedRoles(result);
                return result;
            }
        }

        public bool DeleteIsAllowed(string documentId)
        {
            throw new NotImplementedException();
        }

        public void SoftDelete(string documentId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetIdsOfDocumentsWhichMustBeHardDeletedNow()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //TODO
        }

        public void SetLogConnectionAttemptErrors(bool enabled)
        {
            this._Database.SetLogConnectionAttemptErrors(enabled);
        }

        public void Initialize()
        {
            lock (_Lock)
            {
                try
                {
                    this.InitializationState = new Initializing();
                    this._Database.GetGenericDatabaseInteractor().DoAllMigrations(this._Database.GetAllMigrations(), this._TimeService);
                    this.InitializationState = new Initialized();
                }
                catch (Exception ex)
                {
                    this.InitializationState = new InitializationFailed();
                    this._Log.Log("Initialization failed", ex);
                }
            }
        }

        public void Reset()
        {
            this.RunTransaction(nameof(Reset), (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptResetDatabase();
                cmd.ExecuteNonQuery();
            });
        }
    }
}
