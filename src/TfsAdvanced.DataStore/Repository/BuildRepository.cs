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

        public Build GetBuildBySourceVersion(string commitId)
        {
            return base.GetList(b => b.SourceCommit == commitId).OrderByDescending(b => b.Id).FirstOrDefault();
        }

        protected override int GetId(Build item)
        {
            return item.Id;
        }

        public override void Update(IEnumerable<Build> updates)
        {
            base.Update(updates);
            DateTime yesterday = DateTime.Now.Date.AddDays(-2);
            base.Cleanup(x => x.QueuedDate < yesterday);
        }
    }
    
}
