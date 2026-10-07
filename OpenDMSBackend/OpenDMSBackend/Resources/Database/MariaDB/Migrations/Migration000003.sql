-- Settings which belong to a single user (in contrast to the table `Settings`, whose values apply to the whole installation).
-- The table is a generic key-value-store so that a further user-specific setting does not need another migration.
-- Currently used key: "Theme" with the values "system", "light" and "dark".
CREATE TABLE `UserSettings` (
    `UserId` varchar(255) not null,
    `Key` varchar(255) not null,
    `Value` longtext not null COLLATE utf8mb4_unicode_ci,
    CONSTRAINT `PK_UserSettings` PRIMARY KEY (`UserId`, `Key`)
) CHARACTER SET=utf8mb4;
