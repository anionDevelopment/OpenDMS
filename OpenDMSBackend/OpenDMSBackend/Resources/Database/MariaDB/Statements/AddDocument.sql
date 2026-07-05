
insert 
	into Documents(`Id`, `Title`, `Filename`, `OriginalFilename`, `ImportDate`, `IsLatestVersion`, `IsHardDeleted`, `ReadableId`, `MIMEType`, `OCRContent`, `IsSoftDeleted`,`DeleteIsNotAllowedBefore`,`MustBeHardDeletedAfter`,`GroupOfBusinessOwner`, `AssignedLanguages`,`AddedByUserId`)
	values        (@Id , @Title , @Filename , @OriginalFilename , @ImportDate , @IsLatestVersion , @IsHardDeleted , @ReadableId , @MIMEType , @OCRContent , @IsSoftDeleted, @DeleteIsNotAllowedBefore, @MustBeHardDeletedAfter, @GroupOfBusinessOwner,  @AssignedLanguages, @AddedByUserId);
