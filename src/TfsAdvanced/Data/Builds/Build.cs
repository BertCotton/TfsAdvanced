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
        public DateTime queueTime { get; set; }
        public BuildResult result { get; set; }
        public DateTime? startTime { get; set; }
        public BuildStatus status { get; set; }
        public string sourceBranch { get; set; }
        public string sourceVersion { get; set; }
        public string url { get; set; }
        public BuildLinks _links { get; set; }
        public RequestedFor requestedFor { get; set; }
    }
}