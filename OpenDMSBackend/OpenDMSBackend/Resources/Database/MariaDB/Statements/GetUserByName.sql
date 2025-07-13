
select `Id`, `Name`, `PasswordHash`, `EMailAddress`, `UserIsActivated`, `UserIsLocked`, `RegistrationMoment`, `TOTPActivated`, `TOTPSecretKey` from Users where `Name`=@Name;
