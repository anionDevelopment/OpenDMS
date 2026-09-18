namespace OpenDMSBackend.Core.Model.DTOs
{
    /// <summary>Data transfer object used to create a new tag.</summary>
    public class TagCreationDTO
    {
        /// <summary>The display-name of the tag to create.</summary>
        public string Name { get; set; }
        /// <summary>The color of the tag as a six-digit hexadecimal rgb-value (for example "C62828"), optionally prefixed with a number-sign.</summary>
        public string ColorCode { get; set; }

        /// <summary>Initializes a new instance of <see cref="TagCreationDTO"/>.</summary>
        /// <param name="name">The display-name of the tag to create.</param>
        /// <param name="colorCode">The color of the tag as a six-digit hexadecimal rgb-value.</param>
        public TagCreationDTO(string name, string colorCode)
        {
            this.Name = name;
            this.ColorCode = colorCode;
        }
    }
}
