-- the type of a metadata-field is not changeable, because the values which the documents already hold for it were validated against it.
update "MetadataFieldDefinitions"
    set "Name"=@Name
    where "Id"=@Id;
