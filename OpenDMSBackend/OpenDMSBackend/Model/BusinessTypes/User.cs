using GRYLibrary.Core.APIServer.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    public class User : GRYLibrary.Core.APIServer.CommonDBTypes.User
    {
        public ISet<IStorageLocation> StorageLocations { get; set; } = new HashSet<IStorageLocation>();

        /// <summary>
        /// The id of the configured OIDC provider this user belongs to (e.g. "keycloak-1").
        /// Null for native OpenDMS accounts.
        /// </summary>
        public string? ExternalLoginProvider { get; set; } = null;

        /// <summary>
        /// The subject (sub) claim from the OIDC provider that uniquely identifies this user there.
        /// Null for native OpenDMS accounts.
        /// </summary>
        public string? ExternalLoginSubject { get; set; } = null;

        internal static User Create(string username, string passwordHash, ITimeService timeService)
        {
            User user = CreateNewUser(new User(), username, passwordHash, timeService);
            user.EMailAddress = null;
            user.TOTP = new GRYLibrary.Core.APIServer.MFA.TOTP() { IsActicated = false, SecretKey = Guid.NewGuid().ToString("N") };
            return user;
        }

        internal static User CreateExternalUser(string username, string providerId, string subject, ITimeService timeService)
        {
            User user = CreateNewUser(new User(), username, string.Empty, timeService);
            user.PasswordHash = null;
            user.ExternalLoginProvider = providerId;
            user.ExternalLoginSubject = subject;
            return user;
        }
    }
}
