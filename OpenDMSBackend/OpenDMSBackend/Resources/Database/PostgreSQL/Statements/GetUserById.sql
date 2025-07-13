
select "Id", "Name", "PasswordHash", "EMailAddress", "UserIsActivated", "UserIsLocked", "RegistrationMoment", "TOTPActivated", "TOTPSecretKey" from "Users" where "Id"=$1;
