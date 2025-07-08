
insert 
	into Documents(`Id`, `Title`, `Filename`, `OriginalFilename`, `ImportDate`, `LastEditDate`, `ReadableId`, `MIMEType`, `DocumentContent`, `OCRContent`, `DocumentPreview`) 
	values        (@Id , @Title , @Filename , @OriginalFilename , @ImportDate , @LastEditDate , @ReadableId , @MIMEType , @DocumentContent, @OCRContent  , @DocumentPreview );
