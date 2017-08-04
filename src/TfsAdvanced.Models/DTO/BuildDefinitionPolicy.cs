namespace TFSAdvanced.Models.DTO
{
    public class BuildDefinitionPolicy
    {
        public string Id { get; set; }
        public int BuildDefinitionId { get; set; }

        public int MinimumApproverCount { get; set; }
    }
}
