
CREATE TABLE "Users" (
    "Id" varchar(255) not null,
    "Name" varchar(255) not null,
    "PasswordHash" varchar(255) null,-- when this properties are null then this means that the user uses an external authenticationprovider (for example OpenID) to login.
    "EMailAddress" varchar(255) null,
    "UserIsActivated" boolean not null,
    "UserIsLocked" boolean not null,
    "RegistrationMoment" timestamp with time zone not null,
    "TOTPActivated" boolean null,
    "TOTPSecretKey" varchar(255) null,
    unique("Name"),
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
) ;

CREATE TABLE "StorageLocations" (
    "Id" varchar(255) not null,
    "Name" varchar(255) not null,
    CONSTRAINT "PK_StorageLocations" PRIMARY KEY ("Id")
) ;

CREATE TABLE "Folders" (
    "Id" varchar(255) not null,
    "Name" varchar(255) not null,
    CONSTRAINT "PK_Folders" PRIMARY KEY ("Id")
) ;

CREATE TABLE "Documents" (
    "Id" varchar(255) not null,
    "Title" varchar(255) not null,
    "Filename" varchar(255) not null,
    "OriginalFilename" varchar(255) not null,
    "ImportDate" timestamp with time zone not null,
    "LastEditDate" timestamp with time zone null,
    "ReadableId" BIGINT not null,
    "MIMEType" varchar(255) not null,
    "DocumentContent" bytea not null,
    "OCRContent" text not null,
    "DocumentPreview" bytea not null,
    "IsSoftDeleted" boolean not null,
    "DeleteIsNotAllowedBefore" timestamp with time zone null,
    "MustBeHardDeletedAfter" timestamp with time zone null,
    "GroupOfBusinessOwner" varchar(255) not null,
    "Version" varchar(255) not null,
    "AssignedLanguages" varchar(255) null,
    CONSTRAINT "PK_Documents" PRIMARY KEY ("Id")
) ;

CREATE TABLE "Tags" (
    "Id" varchar(255) not null,
    "Name" varchar(255) not null,
    "NameLower" varchar(255) not null,
    "Color" varchar(8) not null,
    CONSTRAINT "PK_Tags" PRIMARY KEY ("Id")
) ;

CREATE TABLE "Document_Tag" (
    "DocumentId" varchar(255) not null,
    "TagId" varchar(255) not null,
    CONSTRAINT "PK_Document_Tag" PRIMARY KEY ("DocumentId", "TagId")
) ;

CREATE TABLE "Container_Containee" (
    "ContainerId" varchar(255) not null,
    "ContaineeId" varchar(255) not null,
    CONSTRAINT "PK_Container_Containee" PRIMARY KEY ("ContainerId", "ContaineeId")
) ;

CREATE TABLE "AccessToken" (
    "Value" varchar(255) not null,
    "ExpiredMoment" timestamp with time zone not null,
    "UserId" varchar(255) not null,
    CONSTRAINT "PK_AccessToken" PRIMARY KEY ("Value"),
    CONSTRAINT "FK_AccessToken_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
) ;

CREATE TABLE "RefreshToken" (
    "Value" varchar(255) not null,
    "ExpiredMoment" timestamp with time zone not null,
    "UserId" varchar(255) not null,
    CONSTRAINT "PK_RefreshToken" PRIMARY KEY ("Value"),
    CONSTRAINT "FK_RefreshToken_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
) ;

CREATE TABLE "Roles" (
    "Id" varchar(255) not null,
    "Name" varchar(255) not null,
    CONSTRAINT "PK_Roles" PRIMARY KEY ("Id"),
    unique("Name")
) ;

CREATE TABLE "User_Roles" (
    "UserId" varchar(255) not null,
    "RoleId" varchar(255) not null,
    CONSTRAINT "FK_User_Roles_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id"),
    CONSTRAINT "FK_User_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id"),
    CONSTRAINT "PK_Role" PRIMARY KEY ("UserId", "RoleId")
) ;

CREATE TABLE "Role_InheritedRoles" (
    "RoleId" varchar(255) not null,
    "InheritedRoleId" varchar(255) not null,
    CONSTRAINT "FK_Role_InheritedRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id"),
    CONSTRAINT "FK_Role_InheritedRoles_InheritedRoleId" FOREIGN KEY ("InheritedRoleId") REFERENCES "Roles"("Id"),
    CONSTRAINT "PK_Role_InheritedRoles" PRIMARY KEY ("RoleId", "InheritedRoleId")
) ;

CREATE TABLE "StorageLocations_User" (
    "StorageLocationId" varchar(255) not null,
    "UserId" varchar(255) not null,
    CONSTRAINT "FK_User_StorageLocations_StorageLocationsId" FOREIGN KEY ("StorageLocationId") REFERENCES "StorageLocations"("Id"),
    CONSTRAINT "FK_User_StorageLocations_UsersId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id"),
    CONSTRAINT "PK_User_StorageLocations" PRIMARY KEY ("UserId", "StorageLocationId")
) ;
