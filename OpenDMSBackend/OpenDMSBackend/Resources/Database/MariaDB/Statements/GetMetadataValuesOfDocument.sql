
select `MetadataFieldDefinitionId`, `Value`
    from `Document_MetadataValue`
    where `DocumentId`=@DocumentId;
