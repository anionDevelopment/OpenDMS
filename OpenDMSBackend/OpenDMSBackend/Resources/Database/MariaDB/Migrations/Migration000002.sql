ALTER TABLE `Users` ADD COLUMN IF NOT EXISTS `ExternalLoginProvider` varchar(255) null;
ALTER TABLE `Users` ADD COLUMN IF NOT EXISTS `ExternalLoginSubject` varchar(255) null;
