
-- the binary content and the preview of a document are not stored in the database but in the data-folder of the file-system, so they are not part of this statement.
update Documents
	set `Title`=@Title,
		`Filename`=@Filename,
		`MIMEType`=@MIMEType,
		`OCRContent`=@OCRContent,
		`IsSoftDeleted`=@IsSoftDeleted,
		`DeleteIsNotAllowedBefore`=@DeleteIsNotAllowedBefore,
		`MustBeHardDeletedAfter`=@MustBeHardDeletedAfter,
		`GroupOfBusinessOwner`=@GroupOfBusinessOwner,
		`AssignedLanguages`=@AssignedLanguages
	where `Id`=@Id;
