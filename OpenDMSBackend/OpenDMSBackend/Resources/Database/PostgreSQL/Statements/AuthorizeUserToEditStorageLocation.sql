
insert into "StorageLocation_UserPermission"("StorageLocationId","UserId","CanEdit") values(@StorageLocationId,@UserId,true)
	on conflict ("StorageLocationId","UserId") do update set "CanEdit"=true;
