
select `OldDocumentId`
	from DocumentVersionLink
	where `NewDocumentId`=@DocumentId;
