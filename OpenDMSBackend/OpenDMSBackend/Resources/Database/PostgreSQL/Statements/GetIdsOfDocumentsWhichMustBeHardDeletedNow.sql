
select "Id"
	from "Documents"
	where "MustBeHardDeletedAfter" is not null
	  and "MustBeHardDeletedAfter" <= @Now
	  and "IsHardDeleted"=false;
