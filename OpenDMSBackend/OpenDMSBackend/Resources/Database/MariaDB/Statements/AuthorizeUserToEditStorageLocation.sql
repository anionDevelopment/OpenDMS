
insert into StorageLocation_UserPermission(`StorageLocationId`,`UserId`,`CanEdit`) values(@StorageLocationId,@UserId,1)
	on duplicate key update `CanEdit`=1;
