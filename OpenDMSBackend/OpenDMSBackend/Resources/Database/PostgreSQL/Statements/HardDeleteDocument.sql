
update "Documents"
	set "OCRContent"='',
		"AISummaryShort"=null,
		"AISummaryLong"=null,
		"IsHardDeleted"=true
	where "Id"=@Id;
