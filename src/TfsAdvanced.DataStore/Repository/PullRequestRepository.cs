using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TfsAdvanced.Models.PullRequests;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.DataStore.Repository
{
    public class PullRequestRepository : RepositoryBase<PullRequest>
    {

        public IEnumerable<PullRequest> GetPullRequestsAfter(int id)
        {
            return base.GetList(x => x.pullRequestId > id);
        }

        protected override int GetId(PullRequest item)
        {
            return item.pullRequestId;
        }
    }
    
}
