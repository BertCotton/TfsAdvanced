using System.Collections.Generic;
using TfsAdvanced.Data.Policy;
using TfsAdvanced.Data.Projects;

namespace TfsAdvanced.Data.Repositories
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
        public IList<PolicyConfiguration> policyConfigurations { get; set; }
    }
}