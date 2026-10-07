-- the user a tag belongs to is not changeable, so only the name and the color are updated.
update "Tags"
    set "Name"=@Name,
        "Color"=@Color
    where "Id"=@Id;
