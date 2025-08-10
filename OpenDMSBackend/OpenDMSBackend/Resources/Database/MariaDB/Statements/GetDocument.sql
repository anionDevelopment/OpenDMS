
select `Title`, `Filename`, `OriginalFilename`, `ImportDate`, `LastEditDate`, `ReadableId`, `MIMEType`, `DocumentPreview`, `OCRContent`, `DocumentContent`,`IsSoftDeleted`,`DeleteIsNotAllowedBefore`,`MustBeHardDeletedAfter`,`GroupOfBusinessOwner`,`Version`, `AssignedLanguages` from Documents where `Id`=@Id;
