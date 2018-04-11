using System;
using System.Collections.Generic;
using System.Linq;
using Redbus.Interfaces;
using TFSAdvanced.DataStore.Messages;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class BuildRepository : RepositoryBase<Build, BuildUpdateMessage>
    {
        public BuildRepository(IServiceProvider serviceProvider, IEventBus eventBus) : base(serviceProvider, eventBus)
        {
        }

        public Build GetBuild(int buildId)
        {
            return base.Get(b => b.Id == buildId);
        }

        public Build GetBuildBySourceVersion(TFSAdvanced.Models.DTO.Repository repository, string commitId)
        {
            var build = base.GetList(b => b.SourceCommit == commitId).OrderByDescending(b => b.Id).FirstOrDefault();
            if (build == null)
            {
                var repositoryBuilds = base.GetList(b => b.Repository.Id == repository.Id).ToList();
                foreach (var repositoryBuild in repositoryBuilds)
                {
                    if (repositoryBuild.SourceCommit == commitId)
                        build = repositoryBuild;
                }
            }
            return build;
        }

        public IList<Build> GetLatestPerRepository()
        {
            return base.GetAll().GroupBy(x => x.Repository).Select(x => x.OrderByDescending(y => y.QueuedDate).Where(z => z.BuildStatus != BuildStatus.Building && z.BuildStatus != BuildStatus.NotStarted).FirstOrDefault()).ToList();
        }

        protected override int GetId(Build item)
        {
            return item.Id;
        }

        public override bool Update(IEnumerable<Build> updates)
        {
            var updated = base.Update(updates);
            DateTime yesterday = DateTime.Now.Date.AddDays(-2);
            base.CleanupIfNeeded(x => x.QueuedDate < yesterday);
            return updated;
        }
    }
}