using System;

namespace TfsAdvanced.Data
{
    public enum BuildResult
    {
        failed,
        succeeded,
        partiallySucceeded,
        canceled
    }

    public enum BuildStatus
    {
        inProgress,
        completed,
        cancelling,
        postponed,
        notStarted,
        all
    }

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

    public class BuildDefinition
    {
        public int id { get; set; }
        public string name { get; set; }
        public Project project { get; set; }
        public string url { get; set; }
    }

    public class BuildQueueRequest
    {
        public Id definition { get; set; }
        public ProjectGuid project { get; set; }
        public Id queue => new Id() { id = 2 };
        public int reason => 1;
        public string sourceBranch => "refs/heads/develop";
    }

    public class Id
    {
        public int id { get; set; }
    }

    public class ProjectGuid
    {
        public string id { get; set; }
    }
}