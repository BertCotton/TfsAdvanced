using System;
using TFSAdvanced.Aggregator.Raw.Models.Builds;

namespace TFSAdvanced.Aggregator.Raw.Models.JobRequests
{
    public class JobRequest
    {
        public int requestId { get; set; }
        public DateTime queueTime { get; set; }
        public DateTime? assignTime { get; set; }
        public DateTime? receiveTime { get; set; }
        public DateTime? finishTime { get; set; }
        public DateTime? lockedUntil { get; set; }
        public Guid serviceOwner { get; set; }
        public Guid hostId { get; set; }
        public Guid scopeId { get; set; }
        public PlanTypes planType { get; set; }
        public Guid jobId { get; set; }
        public string[] demands { get; set; }
        public Agent reservedAgent { get; set; }
        public BuildDefinition definition { get; set; }
        public Build owner { get; set; }
        public BuildResult result { get; set; }
        public JobRequestStatus status { get; set; }

    }
}
