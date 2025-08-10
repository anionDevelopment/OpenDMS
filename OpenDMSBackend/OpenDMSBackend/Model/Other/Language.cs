namespace OpenDMSBackend.Core.Model.Other
{
    public class Language
    {
        public string Name { get; set; }
        public string ISO639_1_Name { get; set; }
        public string ISO639_3_Name { get; set; }

        public Language(string name, string iSO639_1_Name, string iSO639_3_Name)
        {
            this.Name = name;
            this.ISO639_1_Name = iSO639_1_Name;
            this.ISO639_3_Name = iSO639_3_Name;
        }
    }
}
