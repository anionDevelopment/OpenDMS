-- Settings which belong to a single user (in contrast to the table "Settings", whose values apply to the whole installation).
-- The table is a generic key-value-store so that a further user-specific setting does not need another migration.
-- Currently used key: "Theme" with the values "system", "light" and "dark".
CREATE TABLE "UserSettings" (
    "UserId" varchar(255) not null,
    "Key" varchar(255) not null,
    "Value" text not null,
    CONSTRAINT "PK_UserSettings" PRIMARY KEY ("UserId", "Key")
);
