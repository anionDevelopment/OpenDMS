
select `Title`, `Filename`, `OriginalFilename`, `ImportDate`, `IsLatestVersion`, `ReadableId`, `MIMEType`, `IsSoftDeleted`,`DeleteIsNotAllowedBefore`,`MustBeHardDeletedAfter`,`GroupOfBusinessOwner`, `AssignedLanguages`, `AddedByUserId`, `AISummaryShort`
	from Documents 
	where `Id`=@Id;
