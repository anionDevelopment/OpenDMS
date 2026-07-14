
select "Id", "Name", "Type"
    from "MetadataFieldDefinitions"
    where "StorageLocationId"=@StorageLocationId;
