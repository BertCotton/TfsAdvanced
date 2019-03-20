using TFSAdvanced.Aggregator.Raw.Models.Projects;
using TFSAdvanced.Aggregator.Raw.Models.Repositories;

namespace TFSAdvanced.Aggregator.Raw.Models.Builds
{
    public class BuildDefinition
    {
        public int id { get; set; }
        public string path { get; set; }
        public string name { get; set; }
        public Project project { get; set; }
        public string url { get; set; }
        public Repository repository { get; set; }
        public BuildQueue queue { get; set; }
        public BuildDefinitionLinks _links { get; set; }
        
    }
}