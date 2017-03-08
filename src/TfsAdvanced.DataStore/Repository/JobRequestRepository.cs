using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TfsAdvanced.Models.JobRequests;

namespace TfsAdvanced.DataStore.Repository
{
    public class JobRequestRepository
    {
        private ConcurrentBag<JobRequest> jobRequests;

        public JobRequestRepository()
        {
            this.jobRequests = new ConcurrentBag<JobRequest>();
        }

        public IList<JobRequest> GetJobRequests(DateTime? fromDate = null, DateTime? toDate = null)
        {
            if(fromDate.HasValue && toDate.HasValue)
              return jobRequests.Where(x => x.queueTime >= fromDate.Value && x.queueTime <= toDate.Value).ToList();
            if (fromDate.HasValue)
                return jobRequests.Where(x => x.queueTime >= fromDate.Value).ToList();
            if(toDate.HasValue)
                return jobRequests.Where(x => x.queueTime <= toDate.Value).ToList();

            return jobRequests.ToList();
        }

        public void UpdateJobRequests(IList<JobRequest> jobRequests)
        {
            this.jobRequests = new ConcurrentBag<JobRequest>(jobRequests);
        }
    }
}
