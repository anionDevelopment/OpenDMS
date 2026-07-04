
update "Documents"
	set "AISummaryShort"=@AISummaryShort,
		"AISummaryLong"=@AISummaryLong
	where "Id"=@Id;
