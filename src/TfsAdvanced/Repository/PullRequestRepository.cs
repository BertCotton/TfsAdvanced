using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Data.PullRequests;

namespace TfsAdvanced.Repository
{
    public class PullRequestRepository
    {
        private ConcurrentBag<PullRequest> pullRequests;
        

        public PullRequestRepository()
        {
            this.pullRequests = new ConcurrentBag<PullRequest>();
        }

        public IList<PullRequest> GetPullRequests()
        {

            return pullRequests.ToList();
        }

        public IList<PullRequest> GetPullRequestsAfter(int id)
        {

            return pullRequests.Where(x => x.pullRequestId > id).ToList();
        }

        public void UpdatePullRequests(IList<PullRequest> updatedPullRequests)
        {
            this.pullRequests = new ConcurrentBag<PullRequest>(updatedPullRequests);
        }
        
    }
}
