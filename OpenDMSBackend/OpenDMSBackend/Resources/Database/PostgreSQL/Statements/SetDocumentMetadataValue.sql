
insert
    into "Document_MetadataValue"("DocumentId", "MetadataFieldDefinitionId", "Value")
    values (@DocumentId, @MetadataFieldDefinitionId, @Value)
    on conflict ("DocumentId", "MetadataFieldDefinitionId") do update set "Value" = excluded."Value";
