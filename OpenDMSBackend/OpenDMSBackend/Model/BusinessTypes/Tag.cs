using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Model.DTOs;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    public class Tag//TODO there should be global tags (managable by admins) and user-defined tags which are only visible for the user
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ExtendedColor Color { get; set; }
        public Tag(string id, string name, ExtendedColor color)
        {
            this.Id = id;
            this.Name = name;
            this.Color = color;
        }

        internal TagDTO ToDTO()
        {
            return new TagDTO(this.Id, this.Name, this.Color.GetRGBString());
        }
    }
}
