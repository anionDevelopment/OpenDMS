
update "StorageLocation_UserPermission" set "CanEdit"=false where "StorageLocationId"=@StorageLocationId and "UserId"=@UserId;
