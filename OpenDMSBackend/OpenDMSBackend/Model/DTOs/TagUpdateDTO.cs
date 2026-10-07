namespace OpenDMSBackend.Core.Model.DTOs
{
    /// <summary>Data transfer object used to change the name and the color of an existing tag.</summary>
    public class TagUpdateDTO
    {
        /// <summary>The new display-name of the tag.</summary>
        public string Name { get; set; }
        /// <summary>The new color of the tag as a six-digit hexadecimal rgb-value (for example "C62828"), optionally prefixed with a number-sign.</summary>
        public string ColorCode { get; set; }

        /// <summary>Initializes a new instance of <see cref="TagUpdateDTO"/>.</summary>
        /// <param name="name">The new display-name of the tag.</param>
        /// <param name="colorCode">The new color of the tag as a six-digit hexadecimal rgb-value.</param>
        public TagUpdateDTO(string name, string colorCode)
        {
            this.Name = name;
            this.ColorCode = colorCode;
        }
    }
}
