using GRYLibrary.Core.APIServer.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    public class User : GRYLibrary.Core.APIServer.CommonDBTypes.User
    {
        public ISet<IStorageLocation> StorageLocations { get; set; } = new HashSet<IStorageLocation>();
        internal static User Create(string username, string passwordHash, ITimeService timeService)
        {
            User user = CreateNewUser(new User(), username, passwordHash, timeService);
            user.EMailAddress = null;
            user.TOTP = new GRYLibrary.Core.APIServer.MFA.TOTP() { IsActicated = false, SecretKey = Guid.NewGuid().ToString("N") };
            return user;
        }
    }
}
