using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Models.PullRequests;
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

        public override void Update(IEnumerable<PullRequest> updates)
        {
            base.Update(updates);
            // If an update was not received, then remove it
            var noUpdate = base.GetList(request => !updates.Select(x => x.pullRequestId).Contains(request.pullRequestId));
            base.Remove(noUpdate);

        }
    }
    
}
