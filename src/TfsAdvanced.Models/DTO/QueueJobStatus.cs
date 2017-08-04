using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Models.DTO
{
    public enum QueueJobStatus
    {
        Queued,
        Assigned,
        Building,
        Deploying,
        Succeeded,
        Failed,
        Abandonded,
        Cancelled
    }
}
