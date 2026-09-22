using GRYLibrary.Core.Misc;
using OpenDMSBackend.Core.Model.DTOs;
using System;

namespace OpenDMSBackend.Core.Model.BusinessTypes
{
    /// <summary>
    /// A label which can be assigned to arbitrarily many documents to index them. A tag exists independently of the documents it is assigned to and is usable in every storage-location.
    /// A tag is either global (managed by an administrator and usable by everybody) or it belongs to a single user (see <see cref="OwnerUserId"/>).
    /// </summary>
    public class Tag
    {
        /// <summary>The unique identifier of the tag.</summary>
        public string Id { get; set; }
        /// <summary>The display-name of the tag (unique within the tags which are visible for a user).</summary>
        public string Name { get; set; }
        /// <summary>The color the user-interface shows the tag in.</summary>
        public ExtendedColor Color { get; set; }
        /// <summary>The id of the user this tag belongs to, or <see langword="null"/> when it is a global tag which every user can see and use.</summary>
        public string? OwnerUserId { get; set; }

        /// <summary>Initializes a new instance of <see cref="Tag"/>.</summary>
        /// <param name="id">The unique identifier of the tag.</param>
        /// <param name="name">The display-name of the tag.</param>
        /// <param name="color">The color the user-interface shows the tag in.</param>
        /// <param name="ownerUserId">The id of the owning user, or <see langword="null"/> for a global tag.</param>
        public Tag(string id, string name, ExtendedColor color, string? ownerUserId)
        {
            this.Id = id;
            this.Name = name;
            this.Color = color;
            this.OwnerUserId = ownerUserId;
        }

        /// <summary>Indicates whether this tag is visible and usable for the given user, which is the case for a global tag and for a tag the user owns.</summary>
        /// <param name="userId">The id of the user.</param>
        /// <returns><see langword="true"/> if the user may see and use this tag.</returns>
        public bool IsVisibleFor(string userId)
        {
            return this.OwnerUserId == null || this.OwnerUserId == userId;
        }

        internal TagDTO ToDTO()
        {
            return new TagDTO(this.Id, this.Name, this.Color.GetRGBString(), this.OwnerUserId);
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
