
SELECT *
FROM (
  SELECT
    d.Id,
    (
      CASE WHEN d.Title LIKE CONCAT('%', $1, '%') THEN 5 ELSE 0 END +
      CASE WHEN d.Filename LIKE CONCAT('%', $1, '%') THEN 4 ELSE 0 END +
      (
        SELECT COUNT(*)
        FROM Document_Tag dt
        JOIN Tags t ON t.Id = dt.TagId
        WHERE dt.DocumentId = d.Id
          AND t.NameLower LIKE CONCAT('%', $1, '%')
      ) * 3 +
      CASE WHEN d.OriginalFilename LIKE CONCAT('%', $1, '%') THEN 2 ELSE 0 END +
      CASE WHEN d.OCRContent LIKE CONCAT('%', $1, '%') THEN 1 ELSE 0 END
    ) AS score
  FROM "Documents" d
) AS sub
WHERE sub.score > 0
ORDER BY sub.score DESC;