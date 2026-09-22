namespace OpenDMSBackend.Core.Model.DTOs
{
    public class TagDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ColorCode { get; set; }
        /// <summary>The id of the user the tag belongs to, or <see langword="null"/> when it is a global tag which every user can see and use.</summary>
        public string? OwnerUserId { get; set; }

        public TagDTO(string id, string name, string colorCode, string? ownerUserId)
        {
            this.Id = id;
            this.Name = name;
            this.ColorCode = colorCode;
            this.OwnerUserId = ownerUserId;
        }
    }
}
