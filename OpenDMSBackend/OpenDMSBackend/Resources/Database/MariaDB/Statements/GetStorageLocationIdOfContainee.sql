-- walks the containment-hierarchy upwards, starting at the given containee, and returns the id of the storage-location the containee is (transitively) contained in.
-- only the container which actually is a storage-location is returned; the intermediate folders of the chain are skipped.
WITH RECURSIVE hierarchy AS (
    SELECT `ContainerId`, `ContaineeId`
        FROM `Container_Containee`
        WHERE `ContaineeId` = @ContaineeId

    UNION ALL

    SELECT cc.`ContainerId`, cc.`ContaineeId`
        FROM `Container_Containee` cc
        JOIN hierarchy h
            ON cc.`ContaineeId` = h.`ContainerId`
)
SELECT h.`ContainerId`
    FROM hierarchy h
    JOIN `StorageLocations` sl
        ON sl.`Id` = h.`ContainerId`
    LIMIT 1;
