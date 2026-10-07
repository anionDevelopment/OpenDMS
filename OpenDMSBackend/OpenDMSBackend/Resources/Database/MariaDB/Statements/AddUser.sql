
insert
	into Users(`Id`, `Name`, `PasswordHash`, `EMailAddress`, `UserIsActivated`, `UserIsLocked`, `RegistrationMoment`, `TOTPActivated`, `TOTPSecretKey`, `ExternalLoginProvider`, `ExternalLoginSubject`)
	values (@Id, @Name, @PasswordHash, @EMailAddress, @UserIsActivated, @UserIsLocked, @RegistrationMoment, @TOTPActivated, @TOTPSecretKey, @ExternalLoginProvider, @ExternalLoginSubject);
