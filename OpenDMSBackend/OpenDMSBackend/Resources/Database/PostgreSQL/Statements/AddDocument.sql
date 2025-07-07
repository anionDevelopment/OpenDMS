-- MariaDB-syntax
insert 
	into Documents(`Id`, `Title`, `Filename`, `OriginalFilename`, `ImportDate`, `LastEditDate`, `ReadableId`, `MIMEType`, `DocumentContent`, `DocumentPreview`, `OCRContent`) 
	values        (@Id , @Title , @Filename , @OriginalFilename , @ImportDate , @LastEditDate , @ReadableId , @MIMEType , @DocumentContent , @DocumentPreview , @OCRContent );
