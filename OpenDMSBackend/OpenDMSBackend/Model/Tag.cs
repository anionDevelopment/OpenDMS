using System.Drawing;

namespace OpenDMSBackend.Core.Model
{
    public class Tag
    {

        public string Id { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public Tag(string id, string name, Color color)
        {
            this.Name = name;
            this.Color = color;
        }
    }
}
