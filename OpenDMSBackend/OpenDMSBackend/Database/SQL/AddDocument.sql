-- MariaDB-syntax
insert 
	into Documents(`Id`, `Title`, `OwnerId`, `Filename`, `OriginalFilename`, `ImportDate`, `LastEditDate`, `ReadableId`, `DocumentContent`, `DocumentPreview`) 
	values        (@Id , @Title , @OwnerId , @Filename , @OriginalFilename , @ImportDate , @LastEditDate , @ReadableId , @DocumentContent , @DocumentPreview );
