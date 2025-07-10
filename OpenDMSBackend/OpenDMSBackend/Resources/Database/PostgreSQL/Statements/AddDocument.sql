
insert 
	into "Documents"("Id", "Title", "Filename", "OriginalFilename", "ImportDate", "LastEditDate", "ReadableId", "MIMEType", "DocumentContent", "OCRContent", "DocumentPreview") 
	values          ($1 , $2 , $3, $4, $5,$6, $7, $8, $9, $10, $11);
