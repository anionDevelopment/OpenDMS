
select "Title", "Filename", "OriginalFilename", "ImportDate", "IsLatestVersion", "ReadableId", "MIMEType", "OCRContent","IsSoftDeleted","DeleteIsNotAllowedBefore","MustBeHardDeletedAfter","GroupOfBusinessOwner","AssignedLanguages","AddedByUserId","AISummaryShort","AISummaryLong","IsHardDeleted"
    from "Documents"
    where "Id"=@Id;
