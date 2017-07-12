using System.Collections.Generic;
using TfsAdvanced.Models.Policy;
using TfsAdvanced.Models.Projects;

namespace TfsAdvanced.Models.Repositories
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
        public IEnumerable<PolicyConfiguration> policyConfigurations { get; set; }
    }
}