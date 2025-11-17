INSERT
	INTO "StorageLocations_User" ("UserId", "StorageLocationId")
	VALUES (@UserId, @StorageLocationId)
	ON CONFLICT ("UserId", "StorageLocationId") DO NOTHING;