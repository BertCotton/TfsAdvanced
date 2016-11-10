using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data.JobRequests;

namespace TfsAdvanced.Repository
{
    public class JobRequestRepository
    {
        private ConcurrentBag<JobRequest> jobRequests;

        public JobRequestRepository()
        {
            this.jobRequests = new ConcurrentBag<JobRequest>();
        }

        public IList<JobRequest> GetJobRequests()
        {
            return jobRequests.ToList();
        }

        public void UpdateJobRequests(IList<JobRequest> jobRequests)
        {
            this.jobRequests = new ConcurrentBag<JobRequest>(jobRequests);
        }
    }
}
