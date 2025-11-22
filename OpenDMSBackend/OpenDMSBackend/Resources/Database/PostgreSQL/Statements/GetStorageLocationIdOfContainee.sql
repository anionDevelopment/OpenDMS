WITH RECURSIVE hierarchy AS (
    SELECT  "ContainerId", "ContaineeId"
        FROM "Container_Containee"
        WHERE "ContaineeId" = @ContaineeId

    UNION ALL

    SELECT   cc."ContainerId", cc."ContaineeId"
        FROM "Container_Containee" cc
        JOIN hierarchy h 
            ON cc."ContaineeId" = h."ContainerId"
)
SELECT "ContainerId"
    FROM hierarchy;