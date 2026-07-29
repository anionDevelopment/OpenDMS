
-- a deleted document (soft- or hard-deleted) and an outdated version are not part of the search-result. this mirrors the filter of BusinessLogicService.Search, which defines the search-semantics for every persistence-implementation; applying it here already avoids scoring and returning documents which would be dropped afterwards anyway.
SELECT *
FROM (
  SELECT
    d.`Id`,
    (
      CASE WHEN d.`Title` LIKE CONCAT('%', @SearchTerm, '%') THEN 5 ELSE 0 END +
      CASE WHEN d.`Filename` LIKE CONCAT('%', @SearchTerm, '%') THEN 4 ELSE 0 END +
      (
        SELECT COUNT(*)
        FROM `Document_Tag` dt
        JOIN `Tags` t ON t.Id = dt.`TagId`
        WHERE dt.`DocumentId` = d.`Id`
          AND t.`Name` LIKE CONCAT('%', @SearchTerm, '%')
      ) * 3 +
      CASE WHEN d.`OriginalFilename` LIKE CONCAT('%', @SearchTerm, '%') THEN 2 ELSE 0 END +
      CASE WHEN d.`OCRContent` LIKE CONCAT('%', @SearchTerm, '%') THEN 1 ELSE 0 END
    ) AS score
  FROM `Documents` d
  WHERE d.`IsSoftDeleted`=0
    AND d.`IsHardDeleted`=0
    AND d.`IsLatestVersion`=1
) AS sub
WHERE sub.score > 0
ORDER BY sub.score DESC;