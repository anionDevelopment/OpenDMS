
select `Title`, `Filename`, `OriginalFilename`, `ImportDate`, `LastEditDate`, `ReadableId`, `MIMEType`, `IsSoftDeleted`,`DeleteIsNotAllowedBefore`,`MustBeHardDeletedAfter`,`GroupOfBusinessOwner`,`Version`, `AssignedLanguages`, `AddedByUserId` 
	from Documents 
	where `Id`=@Id;
