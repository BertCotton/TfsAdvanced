using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TfsAdvanced.Models.Builds;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.DataStore.Repository
{
    public class BuildRepository : RepositoryBase<Build>
    {
        
        public Build GetBuild(int buildId)
        {
            return base.Get(b => b.id == buildId);
        }

        public Build GetBuildBySourceVersion(string commitId)
        {
            return base.GetList(b => b.sourceVersion == commitId).OrderByDescending(b => b.id).FirstOrDefault();
        }

        protected override int GetId(Build item)
        {
            return item.id;
        }

        public override void Update(IEnumerable<Build> updates)
        {
            base.Update(updates);
            DateTime yesterday = DateTime.Now.Date.AddDays(-2);
            base.Cleanup(x => x.queueTime < yesterday);
        }
    }
    
}
