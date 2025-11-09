
insert 
	into "Documents"("Id", "Title", "Filename", "OriginalFilename", "ImportDate", "LastEditDate", "ReadableId", "MIMEType", "DocumentContent", "OCRContent", "DocumentPreview", "IsSoftDeleted", "DeleteIsNotAllowedBefore", "MustBeHardDeletedAfter", "GroupOfBusinessOwner", "Version", "AssignedLanguages", "AddedByUserId") 
	values          ( $1 , $2     , $3        , $4                , $5          , $6            , $7          , $8        , $9               , $10         , $11              , $12            , $13                       , $14                     , $15                   , $16      , $17                , $18)
