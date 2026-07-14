
select `Title`, `Filename`, `OriginalFilename`, `ImportDate`, `IsLatestVersion`, `ReadableId`, `MIMEType`, `IsSoftDeleted`,`DeleteIsNotAllowedBefore`,`MustBeHardDeletedAfter`,`GroupOfBusinessOwner`, `AssignedLanguages`, `AddedByUserId`, `AISummaryShort`, `IsHardDeleted`
	from Documents 
	where `Id`=@Id;
