using System.Collections.Generic;
using TfsAdvanced.Data.Projects;

namespace TfsAdvanced.Data.Builds
{
    public class BuildDefinition
    {
        public int id { get; set; }
        public string name { get; set; }
        public Project project { get; set; }
        public string url { get; set; }
        public string defaultBranch { get; set; }
        public BuildQueue queue { get; set; }

        public IList<Build> LatestBuilds { get; set; }
    }
}