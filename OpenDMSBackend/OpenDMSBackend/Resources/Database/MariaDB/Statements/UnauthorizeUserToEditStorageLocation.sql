
update StorageLocation_UserPermission set `CanEdit`=0 where `StorageLocationId`=@StorageLocationId and `UserId`=@UserId;
