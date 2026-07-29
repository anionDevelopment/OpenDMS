
INSERT
	INTO `Container_Containee` (`ContainerId`, `ContaineeId`)
	VALUES (@ContainerId, @ContaineeId)
	ON DUPLICATE KEY UPDATE `ContainerId`=`ContainerId`;
