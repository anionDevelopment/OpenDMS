namespace OpenDMSBackend.Core.Model.DTOs
{
    public class StringValueDTO
    {
       public string Value { get; set; }

        /// <summary>Initializes a new instance of <see cref="StringValueDTO"/>.</summary>
        /// <param name="value">The wrapped string-value.</param>
        public StringValueDTO(string value)
        {
            this.Value = value;
        }
    }
}
