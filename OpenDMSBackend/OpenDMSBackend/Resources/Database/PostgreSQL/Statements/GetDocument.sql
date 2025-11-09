
select "Title", "Filename", "OriginalFilename", "ImportDate", "LastEditDate", "ReadableId", "MIMEType", "DocumentPreview", "OCRContent", "DocumentContent","IsSoftDeleted","DeleteIsNotAllowedBefore","MustBeHardDeletedAfter","GroupOfBusinessOwner","Version","AssignedLanguages","AddedByUserId"
from "Documents"
where "Id"=$1;
