using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Models.DTO
{
    public class QueueJob
    {
        public int RequestId { get; set; }

        public string Url { get; set; }

        public JobType JobType { get; set; }

        public Project Project { get; set; }

        public User LaunchedBy { get; set; }

        public QueueJobStatus QueueJobStatus { get; set; }

        public DateTime QueuedTime { get; set; }

        public DateTime? AssignedTime { get; set; }

        public DateTime? StartedTime { get; set; }

        public DateTime? FinishedTime { get; set; }
    }
}
