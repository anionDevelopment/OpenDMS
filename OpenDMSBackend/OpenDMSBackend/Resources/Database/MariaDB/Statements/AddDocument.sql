
insert 
	into Documents(`Id`, `Title`, `Filename`, `OriginalFilename`, `ImportDate`, `LastEditDate`, `ReadableId`, `MIMEType`, `OCRContent`, `IsSoftDeleted`,`DeleteIsNotAllowedBefore`,`MustBeHardDeletedAfter`,`GroupOfBusinessOwner`,`Version`, `AssignedLanguages`,`AddedByUserId`)
	values        (@Id , @Title , @Filename , @OriginalFilename , @ImportDate , @LastEditDate , @ReadableId , @MIMEType , @OCRContent , @IsSoftDeleted, @DeleteIsNotAllowedBefore, @MustBeHardDeletedAfter, @GroupOfBusinessOwner, @Version,  @AssignedLanguages, @AddedByUserId);
