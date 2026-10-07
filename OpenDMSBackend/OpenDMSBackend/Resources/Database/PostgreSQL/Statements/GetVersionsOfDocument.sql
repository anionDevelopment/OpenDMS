
select "DocumentId", "ContentId", "Version", "Timestamp"
	from "DocumentVersion"
	where "DocumentId"=@DocumentId
	order by "Version" asc;
