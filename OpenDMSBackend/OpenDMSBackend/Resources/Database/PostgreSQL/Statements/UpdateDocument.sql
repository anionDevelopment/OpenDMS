
update "Documents"
	set "Title"=@Title,
		"Filename"=@Filename,
		"MIMEType"=@MIMEType,
		"DocumentContent"=@DocumentContent,
		"OCRContent"=@OCRContent,
		"DocumentPreview"=@DocumentPreview,
		"IsSoftDeleted"=@IsSoftDeleted,
		"DeleteIsNotAllowedBefore"=@DeleteIsNotAllowedBefore,
		"MustBeHardDeletedAfter"=@MustBeHardDeletedAfter,
		"GroupOfBusinessOwner"=@GroupOfBusinessOwner,
		"AssignedLanguages"=@AssignedLanguages
	where "Id"=@Id;