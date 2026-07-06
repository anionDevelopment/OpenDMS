
CREATE TABLE "Users" (
    "Id" varchar(255) not null,
    "Name" varchar(255) not null,
    "PasswordHash" varchar(255) null,-- when this properties are null then this means that the user uses an external authenticationprovider (for example OpenID) to login.
    "EMailAddress" varchar(255) null,
    "UserIsActivated" boolean not null,
    "UserIsLocked" boolean not null,
    "RegistrationMoment" timestamp not null,
    "TOTPActivated" boolean null,
    "TOTPSecretKey" varchar(255) null,
    unique("Name"),
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "StorageLocations" (
    "Id" varchar(255) not null,
    "Name" varchar(255) not null,
    CONSTRAINT "PK_StorageLocations" PRIMARY KEY ("Id")
);

CREATE TABLE "Folders" (
    "Id" varchar(255) not null,
    "Name" varchar(255) not null,
    CONSTRAINT "PK_Folders" PRIMARY KEY ("Id")
);

CREATE TABLE "Documents" (
    "Id" varchar(255) not null,
    "Title" varchar(255) not null,
    "Filename" varchar(255) not null,
    "OriginalFilename" varchar(255) not null,
    "ImportDate" timestamp not null,
    "ReadableId" BIGINT not null,
    "MIMEType" varchar(255) not null,
    "OCRContent" text not null,
    "IsSoftDeleted" boolean not null,
    "IsLatestVersion" boolean not null,
    "IsHardDeleted" boolean not null,
    "DeleteIsNotAllowedBefore" timestamp null,
    "MustBeHardDeletedAfter" timestamp null,
    "GroupOfBusinessOwner" varchar(255) not null,
    "AssignedLanguages" varchar(255) null,
    "AddedByUserId" varchar(255) null,
    "AISummaryShort" text null,
    "AISummaryLong" text null,
    CONSTRAINT "PK_Documents" PRIMARY KEY ("Id")
);

CREATE TABLE "Settings" (
    "Key" varchar(255) not null,
    "Value" text not null,
    CONSTRAINT "PK_Settings" PRIMARY KEY ("Key")
);

CREATE TABLE "DocumentVersion" (
    "DocumentId" varchar(255) not null,
    "ContentId" varchar(255) not null,
    "Version" integer not null,
    "Timestamp" timestamp not null,
    CONSTRAINT "PK_DocumentVersion" PRIMARY KEY ("ContentId"),
    CONSTRAINT "FK_DocumentVersion_ContentId" FOREIGN KEY ("ContentId") REFERENCES "Documents"("Id")
);

CREATE TABLE "Tags" (
    "Id" varchar(255) not null,
    "Name" varchar(255) not null,
    "Color" integer not null,
    CONSTRAINT "PK_Tags" PRIMARY KEY ("Id")
);

CREATE TABLE "Document_Tag" (
    "DocumentId" varchar(255) not null,
    "TagId" varchar(255) not null,
    CONSTRAINT "PK_Document_Tag" PRIMARY KEY ("DocumentId", "TagId")
);

CREATE TABLE "Container_Containee" (
    "ContainerId" varchar(255) not null,
    "ContaineeId" varchar(255) not null,
    CONSTRAINT "PK_Container_Containee" PRIMARY KEY ("ContainerId", "ContaineeId")
);

CREATE TABLE "AccessToken" (
    "Value" varchar(255) not null,
    "ExpiredMoment" timestamp not null,
    "UserId" varchar(255) not null,
    CONSTRAINT "PK_AccessToken" PRIMARY KEY ("Value"),
    CONSTRAINT "FK_AccessToken_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
);

CREATE TABLE "RefreshToken" (
    "Value" varchar(255) not null,
    "ExpiredMoment" timestamp not null,
    "UserId" varchar(255) not null,
    CONSTRAINT "PK_RefreshToken" PRIMARY KEY ("Value"),
    CONSTRAINT "FK_RefreshToken_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
);

CREATE TABLE "Roles" (
    "Id" varchar(255) not null,
    "Name" varchar(255) not null,
    CONSTRAINT "PK_Roles" PRIMARY KEY ("Id"),
    unique("Name")
);

CREATE TABLE "User_Roles" (
    "UserId" varchar(255) not null,
    "RoleId" varchar(255) not null,
    CONSTRAINT "FK_User_Roles_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id"),
    CONSTRAINT "FK_User_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id"),
    CONSTRAINT "PK_Role" PRIMARY KEY ("UserId", "RoleId")
);

CREATE TABLE "Role_InheritedRoles" (
    "RoleId" varchar(255) not null,
    "InheritedRoleId" varchar(255) not null,
    CONSTRAINT "FK_Role_InheritedRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id"),
    CONSTRAINT "FK_Role_InheritedRoles_InheritedRoleId" FOREIGN KEY ("InheritedRoleId") REFERENCES "Roles"("Id"),
    CONSTRAINT "PK_Role_InheritedRoles" PRIMARY KEY ("RoleId", "InheritedRoleId")
);

CREATE TABLE "StorageLocations_User" (
    "StorageLocationId" varchar(255) not null,
    "UserId" varchar(255) not null,
    CONSTRAINT "FK_User_StorageLocations_StorageLocationsId" FOREIGN KEY ("StorageLocationId") REFERENCES "StorageLocations"("Id"),
    CONSTRAINT "FK_User_StorageLocations_UsersId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id"),
    CONSTRAINT "PK_User_StorageLocations" PRIMARY KEY ("UserId", "StorageLocationId")
);

CREATE TABLE "StorageLocation_UserPermission" (
    "StorageLocationId" varchar(255) not null,
    "UserId" varchar(255) not null,
    "CanEdit" boolean not null,
    CONSTRAINT "FK_SLUP_StorageLocationId" FOREIGN KEY ("StorageLocationId") REFERENCES "StorageLocations"("Id"),
    CONSTRAINT "FK_SLUP_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id"),
    CONSTRAINT "PK_StorageLocation_UserPermission" PRIMARY KEY ("StorageLocationId", "UserId")
);
