using TfsAdvanced.Data.Projects;

namespace TfsAdvanced.Data.Builds
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