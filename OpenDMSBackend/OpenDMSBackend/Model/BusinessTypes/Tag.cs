using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Model.DTOs;
using System;
using System.Drawing;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    public class Tag
    {

        public string Id { get; set; }
        public string Name { get; set; }
        public ExtendedColor Color { get; set; }
        public Tag(string id, string name, ExtendedColor color)
        {
            this.Name = name;
            this.Color = color;
        }

        internal TagDTO ToDTO()
        {
            return new TagDTO(Id, Name, Color.GetRGBString());
        }
    }
}
