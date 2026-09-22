namespace OpenDMSBackend.Core.Model.DTOs
{
    /// <summary>Data transfer object used to create a new tag.</summary>
    public class TagCreationDTO
    {
        /// <summary>The display-name of the tag to create.</summary>
        public string Name { get; set; }
        /// <summary>The color of the tag as a six-digit hexadecimal rgb-value (for example "C62828"), optionally prefixed with a number-sign.</summary>
        public string ColorCode { get; set; }
        /// <summary>Indicates whether the tag is created as a global tag (usable by every user, only an administrator may do this) instead of a tag which belongs to the creating user.</summary>
        public bool IsGlobal { get; set; }

        /// <summary>Initializes a new instance of <see cref="TagCreationDTO"/>.</summary>
        /// <param name="name">The display-name of the tag to create.</param>
        /// <param name="colorCode">The color of the tag as a six-digit hexadecimal rgb-value.</param>
        /// <param name="isGlobal">Indicates whether the tag is created as a global tag instead of a tag which belongs to the creating user.</param>
        public TagCreationDTO(string name, string colorCode, bool isGlobal)
        {
            this.Name = name;
            this.ColorCode = colorCode;
            this.IsGlobal = isGlobal;
        }
    }
}
