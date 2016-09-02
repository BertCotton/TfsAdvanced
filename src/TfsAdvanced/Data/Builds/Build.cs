using System;
using TfsAdvanced.Data.Projects;

namespace TfsAdvanced.Data.Builds
{
    public class Build
    {
        public string buildNumber { get; set; }
        public string buildUrl { get; set; }
        public BuildDefinition definition { get; set; }
        public DateTime? finishTime { get; set; }
        public int id { get; set; }
        public DateTime lastChangedDate { get; set; }
        public Project project { get; set; }
        public DateTime queuedTime { get; set; }
        public BuildResult result { get; set; }
        public DateTime? startTime { get; set; }
        public BuildStatus status { get; set; }
        public string url { get; set; }
    }
}