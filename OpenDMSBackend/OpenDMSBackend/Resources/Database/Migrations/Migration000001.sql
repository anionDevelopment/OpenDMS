-- MariaDB-syntax
ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Users` (
    `Id` varchar(255) not null,
    `Name` varchar(255) not null,
    `PasswordHash` varchar(255) null,-- when this properties are null then this means that the user uses an external authenticationprovider (for example OpenID) to login.
    `EMailAddress` varchar(255) null,
    `UserIsActivated` tinyint(1) not null,
    `UserIsLocked` tinyint(1) not null,
    `RegistrationMoment` datetime(6) not null,
    `TOTPActivated` tinyint(1) null,
    `TOTPSecretKey` varchar(255) null,
    unique(`Name`),
    CONSTRAINT `PK_Users` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `StorageLocations` (
    `Id` varchar(255) not null,
    `Name` varchar(255) not null,
    CONSTRAINT `PK_StorageLocations` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Folders` (
    `Id` varchar(255) not null,
    `Name` varchar(255) not null,
    CONSTRAINT `PK_StorageLocations` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Documents` (
    `Id` varchar(255) not null,
    `Title` varchar(255) not null,
    `Filename` varchar(255) not null,
    `OriginalFilename` varchar(255) not null,
    `ImportDate` datetime(6) not null,
    `LastEditDate` datetime(6) null,
    `ReadableId` BIGINT unsigned not null,
    `MIMEType` varchar(255) not null,
    `DocumentPreview` longblob not null,
    `OCRContent` longtext not null,
    `DocumentContent` longblob not null,
    CONSTRAINT `PK_Documents` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Container_Containee` (
    `ContainerId` varchar(255) not null,
    `ContaineeId` varchar(255) not null
) CHARACTER SET=utf8mb4;

CREATE TABLE `AccessToken` (
    `Value` varchar(255) not null,
    `ExpiredMoment` datetime(6) not null,
    `UserId` varchar(255) not null,
    CONSTRAINT `PK_AccessToken` PRIMARY KEY (`Value`),
    CONSTRAINT `FK_AccessToken_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `RefreshToken` (
    `Value` varchar(255) not null,
    `ExpiredMoment` datetime(6) not null,
    `UserId` varchar(255) not null,
    CONSTRAINT `PK_RefreshToken` PRIMARY KEY (`Value`),
    CONSTRAINT `FK_RefreshToken_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Roles` (
    `Id` varchar(255) not null,
    `Name` varchar(255) not null,
    CONSTRAINT `PK_Roles` PRIMARY KEY (`Id`),
    unique(`Name`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `User_Roles` (
    `UserId` varchar(255) not null,
    `RoleId` varchar(255) not null,
    CONSTRAINT `FK_User_Roles_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`),
    CONSTRAINT `FK_User_Roles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles`(`Id`),
    CONSTRAINT `PK_Role` PRIMARY KEY (`UserId`, `RoleId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Role_InheritedRoles` (
    `RoleId` varchar(255) not null,
    `InheritedRoleId` varchar(255) not null,
    CONSTRAINT `FK_Role_InheritedRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Roles`(`Id`),
    CONSTRAINT `FK_Role_InheritedRoles_InheritedRoleId` FOREIGN KEY (`InheritedRoleId`) REFERENCES `Roles`(`Id`),
    CONSTRAINT `PK_Role_InheritedRoles` PRIMARY KEY (`RoleId`, `InheritedRoleId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `StorageLocations_User` (
    `StorageLocationId` varchar(255) not null,
    `UserId` varchar(255) not null,
    CONSTRAINT `FK_User_StorageLocations_StorageLocationsId` FOREIGN KEY (`StorageLocationId`) REFERENCES `StorageLocations`(`Id`),
    CONSTRAINT `FK_User_StorageLocations_UsersId` FOREIGN KEY (`UserId`) REFERENCES `Users`(`Id`),
    CONSTRAINT `PK_User_StorageLocations` PRIMARY KEY (`UserId`, `StorageLocationId`)
) CHARACTER SET=utf8mb4;
