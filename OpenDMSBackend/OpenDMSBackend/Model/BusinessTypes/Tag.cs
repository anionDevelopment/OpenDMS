using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Model.DTOs;
using System;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// A label which can be assigned to arbitrarily many documents to index them. A tag exists independently of the documents it is assigned to and is usable in every storage-location.
    /// </summary>
    public class Tag//TODO there should be global tags (managable by admins) and user-defined tags which are only visible for the user
    {
        /// <summary>The unique identifier of the tag.</summary>
        public string Id { get; set; }
        /// <summary>The display-name of the tag (unique within the installation).</summary>
        public string Name { get; set; }
        /// <summary>The color the user-interface shows the tag in.</summary>
        public ExtendedColor Color { get; set; }

        /// <summary>Initializes a new instance of <see cref="Tag"/>.</summary>
        /// <param name="id">The unique identifier of the tag.</param>
        /// <param name="name">The display-name of the tag.</param>
        /// <param name="color">The color the user-interface shows the tag in.</param>
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

        //a tag is identified by its id, like every other business-type. Without this, two instances which were loaded separately from the database would count as different tags, so the set of tags of a document could contain the same tag more than once.
        public override bool Equals(object? obj)
        {
            return obj is Tag other && this.Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Id);
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}[{nameof(this.Name)}=\"{this.Name}\", {nameof(this.Id)}=\"{this.Id}\"]";
        }
    }
}
