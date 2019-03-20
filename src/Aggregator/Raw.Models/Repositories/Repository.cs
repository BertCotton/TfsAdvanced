using System.Collections.Generic;
using TFSAdvanced.Aggregator.Raw.Models.Policy;
using TFSAdvanced.Aggregator.Raw.Models.Projects;

namespace TFSAdvanced.Aggregator.Raw.Models.Repositories
{
    public class Repository
    {
        public List<Commit> commits { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string defaultBranch { get; set; }
        public Project project { get; set; }
        public string remoteUrl { get; set; }
        public string url { get; set; }
        public ProjectLinks _links { get; set; }
    }
}