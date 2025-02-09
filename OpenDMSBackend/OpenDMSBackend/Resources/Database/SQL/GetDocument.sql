-- MariaDB-syntax
select `Title`, `Filename`, `OriginalFilename`, `ImportDate`, `LastEditDate`, `ReadableId`, `MIMEType`, `DocumentPreview`, `OCRContent`, `DocumentContent` from Documents where `Id`=@Id;
