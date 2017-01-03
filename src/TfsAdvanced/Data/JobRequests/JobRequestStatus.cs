using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Data.JobRequests
{
    public enum JobRequestStatus
    {
        queued,
        assigned,
        started,
        succeeded,
        failed,
        abandoned,
        canceled,

    }
}
