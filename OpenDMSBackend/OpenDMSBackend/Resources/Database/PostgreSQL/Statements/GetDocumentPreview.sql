
select "Title", "Filename", "OriginalFilename", "ImportDate", "LastEditDate", "ReadableId", "MIMEType", "IsSoftDeleted","DeleteIsNotAllowedBefore","MustBeHardDeletedAfter","GroupOfBusinessOwner","Version", "AssignedLanguages","AddedByUserId","AISummaryShort"
from "Documents"
where "Id"=@Id;
