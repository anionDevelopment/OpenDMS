
insert into "StorageLocation_UserPermission"("StorageLocationId","UserId","CanEdit") values(@StorageLocationId,@UserId,false)
	on conflict ("StorageLocationId","UserId") do nothing;
