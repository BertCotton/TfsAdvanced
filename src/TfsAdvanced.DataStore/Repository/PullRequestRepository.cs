using Redbus;
using Redbus.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using TfsAdvanced.Data.Comparator;
using TFSAdvanced.DataStore.Interfaces;
using TFSAdvanced.DataStore.Messages;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.ComparisonResults;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class PullRequestRepository : RepositoryBase<PullRequest, PullRequestUpdateMessage>, IPullRequestRepository
    {
        private DateTime lastCleanup;
        private TimeSpan cleanupTimeSpan = TimeSpan.FromHours(4);

        public PullRequestRepository(IServiceProvider serviceProvider, IEventBus eventBus) : base(serviceProvider, eventBus)
        {
            lastCleanup = DateTime.Now;
        }

        public IEnumerable<PullRequest> GetPullRequestsAfter(int id)
        {
            return base.GetList(x => x.Id > id);
        }

        protected override int GetId(PullRequest item)
        {
            return item.Id;
        }

        public PullRequestComparison Update(PullRequest updatedPullRequest)
        {
            var existing = base.Get(request => request.Id == updatedPullRequest.Id);
            return PullRequestComparator.Compare(existing, updatedPullRequest);
        }

        public override bool Update(IEnumerable<PullRequest> updates)
        {
            bool updated = base.Update(updates);

            if (lastCleanup.Add(cleanupTimeSpan) < DateTime.Now)
            {
                DateTime yesterday = DateTime.Now.Date.AddDays(-4);
                base.CleanupIfNeeded(request => request.ClosedDate.HasValue && request.ClosedDate < yesterday);
            }
            return updated;
        }

        public IList<PullRequest> GetStale(DateTime update)
        {
            return base.GetList(x => x.LastUpdated < update).ToList();
        }
    }
}