
select `DocumentId`, `ContentId`, `Version`, `Timestamp`
	from DocumentVersion
	where `ContentId`=@ContentId;
