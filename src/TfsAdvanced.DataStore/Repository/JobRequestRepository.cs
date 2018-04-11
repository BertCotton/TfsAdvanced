using System;
using System.Collections.Generic;
using System.Linq;
using Redbus.Interfaces;
using TFSAdvanced.DataStore.Messages;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class JobRequestRepository : RepositoryBase<QueueJob, JobRequestUpdateMessage>
    {
        public JobRequestRepository(IServiceProvider serviceProvider, IEventBus eventBus) : base(serviceProvider, eventBus)
        {
        }

        public IEnumerable<QueueJob> GetJobRequests(DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (fromDate.HasValue && toDate.HasValue)
                return base.GetList(x => x.QueuedTime >= fromDate.Value && x.QueuedTime <= toDate.Value);
            if (fromDate.HasValue)
                return base.GetList(x => x.QueuedTime >= fromDate.Value);
            if (toDate.HasValue)
                return base.GetList(x => x.QueuedTime <= toDate.Value);

            return GetAll();
        }

        public override bool Update(IEnumerable<QueueJob> updates)
        {
            var updated = base.Update(updates);
            DateTime yesterday = DateTime.Now.Date.AddDays(-2);
            base.CleanupIfNeeded(x => x.QueuedTime < yesterday);
            return updated;
        }

        protected override int GetId(QueueJob item)
        {
            return item.RequestId;
        }

        public IList<QueueJob> GetLatestPerRepository()
        {
            ISet<QueueJobStatus> filteredStatuses = new HashSet<QueueJobStatus>
            {
                QueueJobStatus.Assigned,
                QueueJobStatus.Building,
                QueueJobStatus.Queued,
                QueueJobStatus.Deploying
            };
            return base.GetAll().GroupBy(x => x.Name).Select(x => x.OrderByDescending(y => y.QueuedTime).Where(z => !filteredStatuses.Contains(z.QueueJobStatus)).FirstOrDefault()).Where(x => x != null).ToList();
        }
    }
}