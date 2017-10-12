namespace TFSAdvanced.Models.DTO
{
    public class ReleaseDefinition
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Project Project { get; set; }
    }
}
