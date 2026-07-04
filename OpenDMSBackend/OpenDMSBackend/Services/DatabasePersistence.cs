using GRYLibrary.Core.APIServer.CommonAuthenticationTypes;
using GRYLibrary.Core.APIServer.Services.Database;
using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Logger;
using GRYLibrary.Core.APIServer.Settings;
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
using System.IO;
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
        private readonly IApplicationConstants _Constants;
        private readonly IOpenDMSDatabaseInteractor _Database;

        public InitializationState InitializationState { get; private set; }

        /// <summary>Initializes a new database-backed persistence instance.</summary>
        /// <param name="database">The database interactor used to execute SQL commands.</param>
        /// <param name="timeService">The service used to obtain the current time.</param>
        /// <param name="log">The logger for diagnostic output.</param>
        /// <param name="constants">Application-wide constants including the data folder path.</param>
        public DatabasePersistence(IOpenDMSDatabaseInteractor database, ITimeService timeService, IServerLog log, IApplicationConstants constants)
        {
            this._TimeService = timeService;
            this._Database = database;
            this._Log = log.Logger;
            this._SQLProvider = database.GetSQLProvider();
            this.InitializationState = new Uninitialized();
            this._Constants = constants;
        }

        #region AccessDatabase
        protected void RunTransaction(string nameOfAction, bool runTransactional, params Action<DbCommand>[] actions)
        {
            lock (_Lock)
            {
                DBUtilities.RunTransaction<IOpenDMSDatabaseInteractor>(nameOfAction, this._Log, this._Database, runTransactional, actions);
            }
        }

        protected T?[] RunTransaction<T>(string nameOfAction, bool runTransactional, params Func<DbCommand, T?>[] functions)
        {
            lock (_Lock)
            {
                return DBUtilities.RunTransaction<T, IOpenDMSDatabaseInteractor>(nameOfAction, this._Log, this._Database, runTransactional, functions);
            }
        }

        #endregion


        #region Save/Load document binary
        private string GetDocumentsDataFolder()
        {
            string result = Path.Combine(this._Constants.GetDataFolder(), "Documents");
            GUtilities.EnsureDirectoryExists(result);
            return result;
        }
        private void SaveDocument(Document document)
        {
            string dataFilePath = Path.Combine(this.GetDocumentsDataFolder(), "document_" + document.Id + ".content.dat");
            GUtilities.EnsureFileExists(dataFilePath);
            File.WriteAllBytes(dataFilePath, document.Content);

            string previewFilePath = Path.Combine(this.GetDocumentsDataFolder(), "document_" + document.Id + ".preview.dat");
            GUtilities.EnsureFileExists(previewFilePath);
            File.WriteAllBytes(previewFilePath, document.Preview);
        }

        private byte[] LoadDocumentPreview(string id)
        {
            string dataFilePath = Path.Combine(this.GetDocumentsDataFolder(), "document_" + id + ".preview.dat");
            GUtilities.AssertCondition(File.Exists(dataFilePath), $"Document with id {id} could not be loaded.");
            byte[] result = File.ReadAllBytes(dataFilePath);
            return result;
        }

        private byte[] LoadDocument(string id)
        {
            string dataFilePath = Path.Combine(this.GetDocumentsDataFolder(), "document_" + id + ".content.dat");
            GUtilities.AssertCondition(File.Exists(dataFilePath), $"Document with id {id} could not be loaded.");
            byte[] result = File.ReadAllBytes(dataFilePath);
            return result;
        }
        #endregion

        /// <inheritdoc />
        public void CreateDocument(Document document)
        {
            this.RunTransaction(nameof(CreateDocument), true, (command) =>
            {
                this.SaveDocument(document);
                command.CommandText = this._SQLProvider.GetScriptAddDocument();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", document.Id));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Title", document.Title.Value));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Filename", document.Filename.Value));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("OriginalFilename", document.OriginalFilename.Value));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ImportDate", this.ToDateTime(document.ImportDate), typeof(DateTime)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("LastEditDate", this.ToDateTime(document.LastEditDate), typeof(DateTime)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ReadableId", document.ReadableId));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("MIMEType", document.MIMEType.Value));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("OCRContent", document.OCRContent));
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
                return null;
            }
        }

        /// <inheritdoc />
        public IDictionary<string, User> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ISet<Role> GetAllRoles()
        {
            ISet<Role> roles = this.RunTransaction(nameof(GetAllRoles), true, (command) =>
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
            })[0];
            foreach (Role role in roles)
            {
                this.EnrichWithDirectlyInheritedRoles(role);
            }
            return roles;
        }

        private void EnrichWithDirectlyInheritedRoles(Role role)
        {
            //TODO loading inherited roles is very inperformant currently, this should be optimized
            ISet<string> inheritedRoleIds = this.RunTransaction(nameof(EnrichWithDirectlyInheritedRoles), true, (command) =>
            {
                ISet<string> inheritedRoleIdsInternal = new HashSet<string>();
                command.CommandText = this._SQLProvider.GetScriptGetDirectlyInheritedRoleIds();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("RoleId", role.Id));
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        inheritedRoleIdsInternal.Add(reader.GetString(0));
                    }
                    reader.Close();
                    return inheritedRoleIdsInternal;
                }
                ;
            })[0];
            foreach (string inheritedRoleId in inheritedRoleIds)
            {
                role.DirectlyInheritedRoles.Add(this.GetRoleById(inheritedRoleId));
            }
        }

        /// <inheritdoc />
        public void AddRole(Role role)
        {
            this.RunTransaction(nameof(AddRole), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptInsertRole();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", role.Id));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", role.Name));
                command.ExecuteNonQuery();
            }, (command) =>
            {
                //TODO check if inherited roles must be updated
            });
        }

        /// <inheritdoc />
        public void UpdateRole(Role role)
        {
            this.RunTransaction(nameof(UpdateRole), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUpdateRole();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", role.Id));
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", role.Name));
                cmd.ExecuteNonQuery();
            }, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptDeleteDirectlyInheritedRoles();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("RoleId", role.Id));
                cmd.ExecuteNonQuery();
            }, (cmd) =>
            {
                if (role.DirectlyInheritedRoles.Any())
                {
                    List<string> insertLines = new List<string>();
                    uint index = 0;
                    cmd.Parameters.Clear();
                    foreach (Role directlyInheritedRole in role.DirectlyInheritedRoles)
                    {
                        insertLines.Add($"(@RoleId{index}, @InheritedRoleId{index})");//attention: this syntax must always be both: mariadb-compliant and postgresql-compliant
                        cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter($"RoleId{index}", role.Id));
                        cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter($"InheritedRoleId{index}", directlyInheritedRole.Id));
                        index++;
                    }
                    cmd.CommandText = this._SQLProvider.GetScriptAddDirectlyInheritedRoles().Replace("__generated__", string.Join(",\n        ", insertLines));
                    cmd.ExecuteNonQuery();
                }
            });
        }

        /// <inheritdoc />
        public void DeleteRoleByName(string roleName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool AccessTokenExists(string accessToken, out User user)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void AddUser(User user)
        {
            this.RunTransaction(nameof(AddUser) + "_" + user.Id, true, (command) =>
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

        /// <inheritdoc />
        public bool UserWithNameExists(string userName)
        {
            return this.RunTransaction(nameof(UserWithNameExists), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserWithNameExists();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserName", userName));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        /// <inheritdoc />
        public bool UserWithIdExists(string userId)
        {
            return this.RunTransaction(nameof(UserWithIdExists), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserWithIdExists();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter(nameof(userId), userId));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        /// <inheritdoc />
        public User GetUserById(string userId)
        {
            User result = this.RunTransaction(nameof(GetUserById), true, (cmd) =>
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
                    user.PasswordHash = reader.GetString(2);
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
            })[0];
            this.EnrichWithRoles(result);
            this.EnrichWithAccessToken(result);
            this.EnrichWithTOTPToken(result);
            return result;
        }
        private void EnrichWithRoles(User user)
        {
            ISet<Role> allRoles = this.GetAllRoles();
            foreach (Role role in allRoles)
            {
                if (this.UserHasRole(user.Id, role.Id))
                {
                    user.Roles.Add(role);
                }
            }
        }

        /// <inheritdoc />
        public User GetUserByName(string userName)
        {
            lock (_Lock)
            {
                User result = GUtilities.GetValue(this.RunTransaction(nameof(GetUserByName) + "_" + userName, true, (cmd) =>
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
                this.EnrichWithRoles(result);
                this.EnrichWithAccessToken(result);
                this.EnrichWithTOTPToken(result);
                return result;
            }
        }

        private void EnrichWithTOTPToken(User result)
        {
            //TODO
        }

        private void EnrichWithAccessToken(User user)
        {
            this.RunTransaction(nameof(EnrichWithAccessToken), true, (cmd) =>
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

        /// <inheritdoc />
        public void RemoveUser(string userId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool RoleExists(string roleName)
        {
            return this.RunTransaction(nameof(RoleExists), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptRoleExists();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("RoleName", roleName));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        /// <inheritdoc />
        public void AddRoleToUser(string userId, string roleId)
        {
            this.RunTransaction(nameof(AddRoleToUser), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddRoleToUser();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserId", userId));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("RoleId", roleId));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void RemoveRoleFromUser(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool UserHasRole(string userId, string roleId)
        {
            return this.RunTransaction(nameof(UserHasRole), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptUserHasRole();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserId", userId));
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("RoleId", roleId));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            })[0];
        }

        /// <inheritdoc />
        public User GetUserByAccessToken(string accessToken)
        {
            return this.GetUserById(this.GetAccessToken(accessToken).OwnerUserId);
        }

        /// <inheritdoc />
        public void UpdateUser(User user)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public uint GetAmountOfDocuments()
        {
            return this.RunTransaction(nameof(GetAmountOfDocuments), true, (cmd) =>
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


        /// <inheritdoc />
        public (bool, Exception?) IsAvailable()
        {
            lock (_Lock)
            {
                bool result = this._Database.GetGenericDatabaseInteractor().TryGetConnection(out _, out Exception? e);
                return (result, e);
            }
        }

        /// <inheritdoc />
        public Document GetDocument(string id)
        {
            lock (_Lock)
            {
                Document result = GUtilities.GetValue(this.RunTransaction(nameof(GetDocument), true, (cmd) =>
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
                                this.LoadDocument(id),//content
                                reader.GetString(7),//ocrcontent
                                this.LoadDocumentPreview(id),//preview
                                reader.GetBoolean(8),//is soft deleted
                                this.ToNullableDateTimeOffset(DBUtilities.GetNullableValue<DateTime>(reader, 9)),//delete is not allowed before
                                 this.ToNullableDateTimeOffset(DBUtilities.GetNullableValue<DateTime>(reader, 10)),//must be deleted after
                                reader.GetString(11),//businessowner
                                Version3.Parse(reader.GetString(12)),//version
                                Core.Misc.Utilities.StringToLanguagesList(reader.GetString(13)),//languages
                                reader.GetString(14)//userid
                            );
                            document.AISummaryShort = DBUtilities.GetNullableValue<string>(reader, 15);
                            document.AISummaryLong = DBUtilities.GetNullableValue<string>(reader, 16);
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
                this.EnrichWithTags(result);
                return result;
            }
        }


        private void EnrichWithTags(Document document)
        {
            ISet<string> tagIds = this.GetTagIdsOfDocument(document.Id);
            foreach (string tagId in tagIds)
            {
                document.Tags.Add(this.GetTag(tagId));
            }
        }

        /// <inheritdoc />
        public void CreateTag(Tag tag)
        {
            this.RunTransaction(nameof(CreateTag), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddTag();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", tag.Id));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", tag.Name));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Color", tag.Color.ColorCode));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void AssignTag(string documentId, string tagId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void UnassignTag(string documentId, string tagId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public TagDTO[] GetAllTags()
        {
            HashSet<TagDTO> result = new HashSet<TagDTO>();
            this.RunTransaction(nameof(GetAllTags), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptGetAllTags();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tag tag = new Tag(
                            reader.GetString(0),
                            reader.GetString(1),
                            new ExtendedColor(reader.GetInt32(2))
                        );
                        result.Add(tag.ToDTO());
                    }
                }
            });
            return result.ToArray();
        }
        public Tag GetTag(string id)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetTag), true, (command) =>
              {
                  command.CommandText = this._SQLProvider.GetScriptGetTag();
                  command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", id));
                  using DbDataReader reader = command.ExecuteReader();
                  if (reader.HasRows)
                  {
                      reader.Read();
                      return new Tag(
                        reader.GetString(0),
                        reader.GetString(1),
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
            this.RunTransaction(nameof(GetTagIdsOfDocument), true, (command) =>
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

        /// <inheritdoc />
        public ISet<string> GetAllDocumentIds()
        {
            HashSet<string> result = new HashSet<string>();
            this.RunTransaction(nameof(GetAllDocumentIds), true, (command) =>
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

        /// <inheritdoc />
        public string GetIdOfStorageLocationContainedIn(string containeeId)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetIdOfStorageLocationContainedIn), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptGetStorageLocationIdOfContainee();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ContaineeId", containeeId));
                using DbDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    //TODO assert that there is only one result-row in the reader
                    reader.Read();
                    return reader.GetString(0);
                }
                else
                {
                    throw new KeyNotFoundException($"No storage-location found for containee with id '{containeeId}'.");
                }
            })[0]);
        }

        /// <inheritdoc />
        public bool UserIsOwnerOfStorageLocation(string userId, string storageLocationId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool StorageLocationIsSharedWithUser(string storageLocationId, string userId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string AddStoragLocation(string name)
        {
            string id = Guid.NewGuid().ToString();
            this.RunTransaction(nameof(AddStoragLocation), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddStorageLocation();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", id));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", name));
                command.ExecuteNonQuery();
            });
            return id;
        }

        /// <inheritdoc />
        public void SetOwnerOfStorageLocation(string storageLocationId, string userId)
        {
            this.RunTransaction(nameof(SetOwnerOfStorageLocation), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddOrUpdateOwnerOfStorageLocation();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("StorageLocationId", storageLocationId));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserId", userId));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public string AddFolder(string name)
        {
            string id = Guid.NewGuid().ToString();
            this.RunTransaction(nameof(AddFolder), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddFolder();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", id));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", name));
                command.ExecuteNonQuery();
            });
            return id;
        }

        /// <inheritdoc />
        public void SetParentOfContainee(IContainee containee, string parentContainerId)
        {
            this.RunTransaction(nameof(SetParentOfContainee), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptSetParentOfContainee();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ContainerId", parentContainerId));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ContaineeId", containee.Id));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void HardDelete(string containerOrContaineeId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void AuthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void UnauthorizeUserToViewStorageLocation(string storageLocationId, string sharedWithUserId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Rename(string containerId, string newName)
        {
            string script = this.IsFolder(containerId)
                ? this._SQLProvider.GetScriptRenameFolder()
                : this._SQLProvider.GetScriptRenameStorageLocation();
            this.RunTransaction(nameof(Rename), true, (command) =>
            {
                command.CommandText = script;
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", containerId));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", newName));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void Update(string requesterUserId, Document document)
        {
            this.RunTransaction(nameof(Update), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptUpdateDocument();

                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", document.Id));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Title", document.Title.Value));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Filename", document.Filename.Value));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("LastEditDate", this.ToDateTime(document.LastEditDate), typeof(DateTime)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("MIMEType", document.MIMEType.Value));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("OCRContent", document.OCRContent));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("IsSoftDeleted", document.IsSoftDeleted));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("DeleteIsNotAllowedBefore", this.ToDateTime(document.DeleteIsNotAllowedBefore), typeof(DateTime)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("MustBeHardDeletedAfter", this.ToDateTime(document.MustBeHardDeletedAfter), typeof(DateTime)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("GroupOfBusinessOwner", document.GroupOfBusinessOwner));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Version", document.Version.ToString()));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("AssignedLanguages", Core.Misc.Utilities.LanguagesListToString(document.AssignedLanguages)));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public string GetParentIdOfContainee(string containeeId)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetParentIdOfContainee), true, (cmd) =>
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

        /// <inheritdoc />
        public bool IsContaineeId(string id)
        {
            return this.IsDocument(id) || this.IsFolder(id);
        }

        /// <inheritdoc />
        public bool IsStorageLocationId(string id)
        {
            return this.RunTransaction(nameof(IsStorageLocationId), true, (cmd) =>
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

        /// <inheritdoc />
        public DocumentPreview GetDocumentPreview(string id)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetDocumentPreview), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetDocumentPreview();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", id));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    //select "Title", "Filename", "OriginalFilename", "ImportDate", "LastEditDate", "ReadableId", "MIMEType", "DocumentPreview","IsSoftDeleted","DeleteIsNotAllowedBefore","MustBeHardDeletedAfter","GroupOfBusinessOwner","Version", "AssignedLanguages","AddedByUserId"

                    DocumentPreview document = new DocumentPreview(
                        id,//id
                        OneLineString.From(reader.GetString(0)),//title
                        OneLineString.From(reader.GetString(1)),//filename
                        OneLineString.From(reader.GetString(2)),//originalfilename
                        this.ToDateTimeOffset(reader.GetDateTime(3)),//import time
                        this.ToNullableDateTimeOffset(DBUtilities.GetNullableValue<DateTime>(reader, 4)),//updatetime
                        new HashSet<Tag>(),
                        (uint)reader.GetInt32(5),//readable id 
                        OneLineString.From(reader.GetString(6)),//mimetype
                        this.LoadDocumentPreview(id),
                        reader.GetBoolean(7),//isdeleted
                        this.ToNullableDateTimeOffset(DBUtilities.GetNullableValue<DateTime>(reader, 8)),//delete is not allowed before
                        this.ToNullableDateTimeOffset(DBUtilities.GetNullableValue<DateTime>(reader, 9)),//must be deleted after
                        reader.GetString(10),//business owner
                        Version3.Parse(reader.GetString(11)),//version
                        Core.Misc.Utilities.StringToLanguagesList(GUtilities.GetValue(DBUtilities.GetNullableValue<string>(reader, 12))), //languages
                        reader.GetString(13)//creator-user-is
                    );
                    document.AISummaryShort = DBUtilities.GetNullableValue<string>(reader, 14);
                    //TODO load tags
                    return document;
                }
                else
                {
                    throw new KeyNotFoundException($"No document found with document '{id}'");
                }
            })[0]);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAllStorageLocationIds()
        {
            ISet<string> result = GUtilities.GetValue(this.RunTransaction(nameof(GetAllStorageLocationIds), true, (command) =>
            {
                ISet<string> storageLocationIds = new HashSet<string>();
                command.CommandText = this._SQLProvider.GetScriptGetAllStorageLocationIds();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                        storageLocationIds.Add(id);
                    }
                    reader.Close();
                    return storageLocationIds;
                }
                ;
            })[0]);
            return result;
        }

        /// <inheritdoc />
        public StorageLocation GetStorageLocation(string storageLocationId)
        {
            StorageLocation result = GUtilities.GetValue(this.RunTransaction(nameof(GetStorageLocation), true, (command) =>
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
            })[0]);
            this.EnrichWithContainees(result);//this can be optimized regrading to performance: this function loads the full objets (folder and document, recursive) from the database. but in practise, only the ids are required for the requested DTO in most cases and loading the other properties as well is just unnecessary.
            return result;
        }

        /// <inheritdoc />
        public Folder GetFolder(string folderId)
        {
            Folder result = GUtilities.GetValue(this.RunTransaction(nameof(GetFolder), true, (command) =>
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
            })[0]);
            this.EnrichWithContainees(result);
            return result;
        }

        private void EnrichWithContainees(IContainer container)
        {
            container.Content = new HashSet<IContainee>();
            ISet<string> contentIds = GUtilities.GetValue(this.RunTransaction(nameof(EnrichWithContainees), true, (cmd) =>
            {
                ISet<string> idList = new HashSet<string>();
                cmd.CommandText = this._SQLProvider.GetScriptGetContentOfContainer();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ContainerId", container.Id));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        idList.Add(reader.GetString(0));
                    }
                }
                return idList;
            })[0]);
            foreach (string id in contentIds)
            {
                container.Content.Add(this.GetContaineeById(id));
            }
        }

        /// <inheritdoc />
        public bool IsStorageLocation(string contentId)
        {
            return this.RunTransaction(nameof(IsStorageLocation), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsStorageLocation();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", contentId));
                using DbDataReader reader = cmd.ExecuteReader();
                reader.Read();
                return reader.HasRows;
            })[0];
        }

        /// <inheritdoc />
        public bool IsFolder(string contentId)
        {
            return this.RunTransaction(nameof(IsFolder), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsFolder();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", contentId));
                using DbDataReader reader = cmd.ExecuteReader();
                reader.Read();
                return reader.HasRows;
            })[0];
        }

        /// <inheritdoc />
        public bool IsDocument(string contentId)
        {
            return this.RunTransaction(nameof(IsDocument), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptIsDocument();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", contentId));
                using DbDataReader reader = cmd.ExecuteReader();
                reader.Read();
                return reader.HasRows;
            })[0];
        }

        /// <inheritdoc />
        public AccessToken GetAccessToken(string accessToken)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetAccessToken), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetAccessToken();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Value", accessToken));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    AccessToken result = new AccessToken();
                    result.Value = accessToken;
                    result.ExpiredMoment = this.ToDateTimeOffset(reader.GetFieldValue<DateTime>(0));
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
            this.RunTransaction(nameof(AddAccessToken), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptAddAccessToken();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Value", newAccessToken.Value));
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ExpiredMoment", newAccessToken.ExpiredMoment));
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserId", userId));
                cmd.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void RemoveAccessToken(string accessToken)
        {
            this.RunTransaction(nameof(RemoveAccessToken), true, (cmd) =>
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

        /// <inheritdoc />
        public ulong GetLatestReadableId()
        {
            return this.GetAmountOfDocuments();
        }

        /// <inheritdoc />
        public ISet<AccessToken> GetAllAccessTokenOfUser(string userId)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetAllAccessTokenOfUser), true, (cmd) =>
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

        /// <inheritdoc />
        public void RemoveChild(string parentId, string childId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string GetIdFromReadableId(uint readableId)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetIdFromReadableId), true, (cmd) =>
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

        /// <inheritdoc />
        public IList<string> Search(string searchTerm)
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(Search), true, (cmd) =>
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

        /// <inheritdoc />
        public Role GetRoleByName(string roleName)
        {
            Role result = this.RunTransaction(nameof(GetRoleByName), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetRoleByName();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Name", roleName));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    Role role = new Role();
                    role.Id = reader.GetString(0);
                    role.Name = roleName;
                    return role;
                }
                else
                {
                    throw new KeyNotFoundException($"No role found with rolename '{roleName}'");
                }
            })[0];
            this.EnrichWithDirectlyInheritedRoles(result);
            return result;
        }

        /// <inheritdoc />
        public Role GetRoleById(string roleId)
        {
            Role result = this.RunTransaction(nameof(GetRoleById), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptGetRoleById();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", roleId));
                using DbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    Role role = new Role();
                    role.Id = roleId;
                    role.Name = reader.GetString(0);
                    return role;
                }
                else
                {
                    throw new KeyNotFoundException($"No role found with id '{roleId}'");
                }
            })[0];
            this.EnrichWithDirectlyInheritedRoles(result);
            return result;
        }

        /// <inheritdoc />
        public bool DeleteIsAllowed(string documentId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SoftDelete(string documentId)
        {
            this.RunTransaction(nameof(SoftDelete), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptSoftDeleteDocument();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", documentId));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("IsSoftDeleted", true));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void SetAISummary(string documentId, string? shortSummary, string? longSummary)
        {
            this.RunTransaction(nameof(SetAISummary), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptSetAISummary();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Id", documentId));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("AISummaryShort", shortSummary, typeof(string)));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("AISummaryLong", longSummary, typeof(string)));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public string? GetSetting(string key)
        {
            return this.RunTransaction(nameof(GetSetting), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptGetSetting();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Key", key));
                using DbDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return reader.GetString(0);
                }
                else
                {
                    return null;
                }
            })[0];
        }

        /// <inheritdoc />
        public void SetSetting(string key, string value)
        {
            this.RunTransaction(nameof(SetSetting), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptSetSetting();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Key", key));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Value", value));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void AddDocumentVersionLink(string oldDocumentId, string newDocumentId)
        {
            this.RunTransaction(nameof(AddDocumentVersionLink), true, (command) =>
            {
                command.CommandText = this._SQLProvider.GetScriptAddDocumentVersionLink();
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("OldDocumentId", oldDocumentId));
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("NewDocumentId", newDocumentId));
                command.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public ISet<string> GetSupersededDocumentIds()
        {
            return GUtilities.GetValue(this.RunTransaction(nameof(GetSupersededDocumentIds), true, (command) =>
            {
                ISet<string> result = new HashSet<string>();
                command.CommandText = this._SQLProvider.GetScriptGetSupersededDocumentIds();
                using (DbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                    reader.Close();
                }
                return result;
            })[0]);
        }

        /// <inheritdoc />
        public string? GetPreviousVersionId(string documentId)
        {
            return this.GetSingleVersionId(nameof(GetPreviousVersionId), this._SQLProvider.GetScriptGetPreviousVersionId(), documentId);
        }

        /// <inheritdoc />
        public string? GetNextVersionId(string documentId)
        {
            return this.GetSingleVersionId(nameof(GetNextVersionId), this._SQLProvider.GetScriptGetNextVersionId(), documentId);
        }

        private string? GetSingleVersionId(string actionName, string script, string documentId)
        {
            return this.RunTransaction(actionName, true, (command) =>
            {
                command.CommandText = script;
                command.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("DocumentId", documentId));
                using DbDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Read();
                    return reader.GetString(0);
                }
                else
                {
                    return null;
                }
            })[0];
        }

        /// <inheritdoc />
        public IEnumerable<string> GetIdsOfDocumentsWhichMustBeHardDeletedNow()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            //TODO
        }

        /// <inheritdoc />
        public void SetLogConnectionAttemptErrors(bool enabled)
        {
            this._Database.SetLogConnectionAttemptErrors(enabled);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Reset()
        {
            this.RunTransaction(nameof(Reset), false, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptResetDatabase();
                cmd.ExecuteNonQuery();
            });
        }

        /// <inheritdoc />
        public void WaitUntilAvailable(TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void AddAccessToken(AccessToken newAccessToken)
        {
            this.RunTransaction(nameof(AddAccessToken), true, (cmd) =>
            {
                cmd.CommandText = this._SQLProvider.GetScriptAddAccessToken();
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("Value", newAccessToken.Value));
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("ExpiredMoment", newAccessToken.ExpiredMoment));
                cmd.Parameters.Add(this._Database.GetGenericDatabaseInteractor().GetParameter("UserId", newAccessToken.OwnerUserId));
                using DbDataReader reader = cmd.ExecuteReader();
                return reader.HasRows;
            });
        }
    }
}
