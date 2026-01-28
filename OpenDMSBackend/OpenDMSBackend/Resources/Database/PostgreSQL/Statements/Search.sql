
SELECT *
FROM (
  SELECT
    d."Id",
    (
      CASE WHEN d."Title" ILIKE CONCAT('%', @SearchTerm, '%') THEN 5 ELSE 0 END +
      CASE WHEN d."Filename" ILIKE CONCAT('%', @SearchTerm, '%') THEN 4 ELSE 0 END +
      (
        SELECT COUNT(*)
        FROM "Document_Tag" dt
        JOIN "Tags" t ON t."Id" = dt."TagId"
        WHERE dt."DocumentId" = d."Id"
          AND t."Name" ILIKE CONCAT('%', @SearchTerm, '%')
      ) * 3 +
      CASE WHEN d."OriginalFilename" ILIKE CONCAT('%', @SearchTerm, '%') THEN 2 ELSE 0 END +
      CASE WHEN d."OCRContent" ILIKE CONCAT('%', @SearchTerm, '%') THEN 1 ELSE 0 END
    ) AS score
  FROM "Documents" d
) AS sub
WHERE sub.score > 0
ORDER BY sub.score DESC;