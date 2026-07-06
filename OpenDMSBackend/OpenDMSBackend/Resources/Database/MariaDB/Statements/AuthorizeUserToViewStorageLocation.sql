
insert into StorageLocation_UserPermission(`StorageLocationId`,`UserId`,`CanEdit`) values(@StorageLocationId,@UserId,0)
	on duplicate key update `UserId`=`UserId`;
