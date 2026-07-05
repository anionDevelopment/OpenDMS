
select "Title", "Filename", "OriginalFilename", "ImportDate", "IsLatestVersion", "ReadableId", "MIMEType", "OCRContent","IsSoftDeleted","DeleteIsNotAllowedBefore","MustBeHardDeletedAfter","GroupOfBusinessOwner","Version","AssignedLanguages","AddedByUserId","AISummaryShort","AISummaryLong"
    from "Documents"
    where "Id"=@Id;
