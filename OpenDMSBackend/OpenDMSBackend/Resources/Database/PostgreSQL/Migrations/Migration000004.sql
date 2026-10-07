-- The user a tag belongs to. A tag without an owner is a global tag which every user can see and use and which only an administrator can change or delete.
-- Every tag which already exists was created when all tags were installation-wide, so it becomes a global tag.
ALTER TABLE "Tags" ADD COLUMN IF NOT EXISTS "OwnerUserId" varchar(255) null;
