
INSERT
	INTO `StorageLocations_User` (`UserId`, `StorageLocationId`)
	VALUES (@UserId, @StorageLocationId)
	ON DUPLICATE KEY UPDATE `UserId`=`UserId`;
