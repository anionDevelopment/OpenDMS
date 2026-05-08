
select "Id", "Name", "PasswordHash", "EMailAddress", "UserIsActivated", "UserIsLocked", "RegistrationMoment", "TOTPActivated", "TOTPSecretKey", "ExternalLoginProvider", "ExternalLoginSubject"
    from "Users"
    where "Name"=@Name;
