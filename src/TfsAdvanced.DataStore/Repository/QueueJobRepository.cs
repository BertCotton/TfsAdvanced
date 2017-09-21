using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TFSAdvanced.DataStore;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class QueueJobRepository : SqlRepositoryBase<QueueJob>
    {
        public QueueJobRepository(IConfiguration configuration) : base(configuration)
        {
        }

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

        protected override bool Matches(QueueJob source, QueueJob target)
        {
            return source.RequestId == target.RequestId;
        }

        protected override IQueryable<QueueJob> AddIncludes(DbSet<QueueJob> data)
        {
            return data.Include(job => job.LaunchedBy)
                .Include(job => job.Project);
        }

        public override void AttachProperties(TfsAdvancedSqlDataContext dataContext, QueueJob queueJob)
        {
            queueJob.LaunchedBy = GetOrUpdate(dataContext, user => user.UniqueName == queueJob.LaunchedBy.UniqueName, queueJob.LaunchedBy);
            if(queueJob.Project != null)
                queueJob.Project = dataContext.Projects.FirstOrDefault(project => project.ProjectId == queueJob.Project.ProjectId);
        }

        protected override void Map(QueueJob from, QueueJob to)
        {
            to.AssignedTime = from.AssignedTime;
            to.FinishedTime = from.FinishedTime;
            to.QueueJobStatus = from.QueueJobStatus;
            to.QueuedTime = from.QueuedTime;
            to.StartedTime = from.StartedTime;
        }
    }
}
