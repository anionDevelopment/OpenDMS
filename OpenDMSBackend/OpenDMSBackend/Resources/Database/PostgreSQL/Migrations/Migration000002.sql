ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "ExternalLoginProvider" varchar(255) null;
ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "ExternalLoginSubject" varchar(255) null;

-- Storage-location-specific custom metadata-fields (see issue #2 / #14). A moderator of a storage-location can define fields which every contained document can optionally hold a value for.
CREATE TABLE "MetadataFieldDefinitions" (
    "Id" varchar(255) not null,
    "StorageLocationId" varchar(255) not null,
    "Name" varchar(255) not null,
    "Type" varchar(50) not null,-- "String" or "Boolean"
    CONSTRAINT "PK_MetadataFieldDefinitions" PRIMARY KEY ("Id")
);

-- the value a single document (identified by its "Documents"-row-id) holds for a single metadata-field. A boolean-value is stored as "true"/"false".
CREATE TABLE "Document_MetadataValue" (
    "DocumentId" varchar(255) not null,
    "MetadataFieldDefinitionId" varchar(255) not null,
    "Value" text not null,
    CONSTRAINT "PK_Document_MetadataValue" PRIMARY KEY ("DocumentId", "MetadataFieldDefinitionId")
);
