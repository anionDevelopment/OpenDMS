
select `NewDocumentId`
	from DocumentVersionLink
	where `OldDocumentId`=@DocumentId;
