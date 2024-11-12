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

namespace OpenDMSBackend.Core.ServiceInterfaces
{
    public sealed class DatabasePersistence : IPersistence, IAuthenticationServicePersistence<Model.User>
    {
        private readonly DatabaseContext _DatabaseContext;
        private static readonly object _Lock = new object();
        private readonly Semaphore _Semaphore = new Semaphore();
         private readonly ITimeService _TimeService;
        private readonly IGRYLog _Log;

        public DatabasePersistence(DbContextOptions<DatabaseContext> options, IGeneralLogger logger, ITimeService timeService, IDatabaseManager databaseManager,    IGRYLog log)
        {
             this._TimeService = timeService;
            this._DatabaseContext = new DatabaseContext(options, logger, timeService, databaseManager);
            this._Log = log;
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
            throw new NotImplementedException();
        }

        public IDictionary<string, Model.User> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public ISet<Role> GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public void AddRole(Role role)
        {
            throw new NotImplementedException();
        }

        public void UpdateRole(Role role)
        {
            throw new NotImplementedException();
        }

        public void DeleteRoleByName(string roleName)
        {
            throw new NotImplementedException();
        }

        public bool AccessTokenExists(string accessToken, out Model.User user)
        {
            throw new NotImplementedException();
        }

        public void AddUser(Model.User newUser)
        {
            throw new NotImplementedException();
        }

        public bool UserWithNameExists(string userName)
        {
            throw new NotImplementedException();
        }

        public bool UserWithIdExists(string userId)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void AddRoleToUser(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public void RemoveRoleFromUser(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public bool UserHasRole(string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public Model.User GetUserByAccessToken(string accessToken)
        {
            throw new NotImplementedException();
        }

        public void UpdateUser(Model.User user)
        {
            throw new NotImplementedException();
        }

        public void DocumentExists(Guid id)
        {
            throw new NotImplementedException();
        }

        public uint GetAmountOfDocuments()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool IsAvailable()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool UserExistsByName(string adminUserName)
        {
            throw new NotImplementedException();
        }
    }
}
