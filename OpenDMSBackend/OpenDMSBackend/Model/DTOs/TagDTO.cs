namespace OpenDMSBackend.Core.Model.DTOs
{
    public class TagDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ColorCode { get; set; }

        public TagDTO(string id, string name, string colorCode)
        {
            this.Id = id;
            this.Name = name;
            this.ColorCode = colorCode;
        }
    }
}
