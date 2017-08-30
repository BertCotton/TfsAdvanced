using System;
using System.Collections.Generic;
using System.Linq;
using TFSAdvanced.DataStore.Interfaces;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class PullRequestRepository : RepositoryBase<PullRequest>, IPullRequestRepository
    {

        public IEnumerable<PullRequest> GetPullRequestsAfter(int id)
        {
            return base.GetList(x => x.Id > id);
        }

        protected override int GetId(PullRequest item)
        {
            return item.Id;
        }

        public override bool Update(IEnumerable<PullRequest> updates)
        {
            bool updated = base.Update(updates);
            // If an update was not received, then remove it
            var noUpdate = base.GetList(request => !updates.Select(x => x.Id).Contains(request.Id));
            bool removed = base.Remove(noUpdate);
            DateTime yesterday = DateTime.Now.Date.AddDays(-2);
            base.Cleanup(request => request.ClosedDate.HasValue && request.ClosedDate < yesterday );
            return updated || removed;

        }
    }
    
}
