
update Documents
	set `OCRContent`='',
		`AISummaryShort`=null,
		`AISummaryLong`=null,
		`IsHardDeleted`=1
	where `Id`=@Id;
