
insert 
	into Documents(`Id`, `Title`, `Filename`, `OriginalFilename`, `ImportDate`, `LastEditDate`, `ReadableId`, `MIMEType`, `DocumentContent`, `OCRContent`, `DocumentPreview`,`IsSoftDeleted`,`DeleteIsNotAllowedBefore`,`MustBeHardDeletedAfter`,`GroupOfBusinessOwner`,`Version`, `AssignedLanguages`)
	values        (@Id , @Title , @Filename , @OriginalFilename , @ImportDate , @LastEditDate , @ReadableId , @MIMEType , @DocumentContent, @OCRContent , @DocumentPreview , @IsSoftDeleted, @DeleteIsNotAllowedBefore, @MustBeHardDeletedAfter, @GroupOfBusinessOwner, @Version, @AssignedLanguages);
