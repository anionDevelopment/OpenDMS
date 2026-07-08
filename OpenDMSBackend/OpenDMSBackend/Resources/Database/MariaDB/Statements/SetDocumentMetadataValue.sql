
insert
    into `Document_MetadataValue`(`DocumentId`, `MetadataFieldDefinitionId`, `Value`)
    values (@DocumentId, @MetadataFieldDefinitionId, @Value)
    on duplicate key update `Value` = values(`Value`);
