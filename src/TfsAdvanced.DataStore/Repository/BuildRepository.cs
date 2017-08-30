using System;
using System.Collections.Generic;
using System.Linq;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class BuildRepository : RepositoryBase<Build>
    {
        
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
