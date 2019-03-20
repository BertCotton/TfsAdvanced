using TfsAdvanced.Models;
using TFSAdvanced.Aggregator.Raw.Models.Projects;

namespace TFSAdvanced.Aggregator.Raw.Models.Builds
{
    public class BuildQueueRequest
    {
        public Id definition { get; set; }
        public ProjectGuid project { get; set; }
        public Id queue { get; set; }
        public int reason => 1;
        public string sourceBranch { get; set; }
    }
}