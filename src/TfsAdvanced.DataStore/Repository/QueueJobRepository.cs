using System;
using System.Collections.Generic;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class QueueJobRepository : SqlRepositoryBase<QueueJob>
    {
        public IEnumerable<QueueJob> GetJobRequests(DateTime? fromDate = null, DateTime? toDate = null)
        {
            if(fromDate.HasValue && toDate.HasValue)
              return base.GetList(x => x.QueuedTime >= fromDate.Value && x.QueuedTime <= toDate.Value);
            if (fromDate.HasValue)
                return base.GetList(x => x.QueuedTime >= fromDate.Value);
            if(toDate.HasValue)
                return base.GetList(x => x.QueuedTime <= toDate.Value);
            return GetAll();
        }

        protected override void Map(QueueJob from, QueueJob to)
        {
            from.AssignedTime = to.AssignedTime;
            from.FinishedTime = to.FinishedTime;
            from.QueueJobStatus = to.QueueJobStatus;
            from.QueuedTime = to.QueuedTime;
            from.StartedTime = to.StartedTime;
        }
    }
}
