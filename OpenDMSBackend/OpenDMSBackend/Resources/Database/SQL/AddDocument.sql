-- MariaDB-syntax
insert 
	into Documents(`Id`, `Title`, `Filename`, `OriginalFilename`, `ImportDate`, `LastEditDate`, `ReadableId`, `MIMEType`, `DocumentContent`, `DocumentPreview`) 
	values        (@Id , @Title , @Filename , @OriginalFilename , @ImportDate , @LastEditDate , @ReadableId , @MIMEType , @DocumentContent , @DocumentPreview );
