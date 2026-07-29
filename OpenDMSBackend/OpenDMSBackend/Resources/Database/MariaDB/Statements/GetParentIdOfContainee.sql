
select `ContainerId` 
	from `Container_Containee` 
	where `ContaineeId`=@ContaineeId limit 1;
