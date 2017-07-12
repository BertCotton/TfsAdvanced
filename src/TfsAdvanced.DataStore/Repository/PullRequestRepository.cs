using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TfsAdvanced.Models.PullRequests;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.DataStore.Repository
{
    public class PullRequestRepository : RepositoryBase<PullRequest>
    {
        public PullRequestRepository() : base(new PullRequestComparer())
        {
        }

        public IEnumerable<PullRequest> GetPullRequestsAfter(int id)
        {
            return base.Get(() => data.Where(x => x.pullRequestId > id));
        }
    }

    class PullRequestComparer : IEqualityComparer<PullRequest>
    {
        public bool Equals(PullRequest x, PullRequest y)
        {
            return x.pullRequestId == y.pullRequestId;
        }

        public int GetHashCode(PullRequest obj)
        {
            return obj.pullRequestId;
        }
    }
}
