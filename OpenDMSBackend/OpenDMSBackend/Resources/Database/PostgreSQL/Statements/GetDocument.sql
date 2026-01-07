
select "Title", "Filename", "OriginalFilename", "ImportDate", "LastEditDate", "ReadableId", "MIMEType", "OCRContent","IsSoftDeleted","DeleteIsNotAllowedBefore","MustBeHardDeletedAfter","GroupOfBusinessOwner","Version","AssignedLanguages","AddedByUserId"
    from "Documents"
    where "Id"=@Id;
