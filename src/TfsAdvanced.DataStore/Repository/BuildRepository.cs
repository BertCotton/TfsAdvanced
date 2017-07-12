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
        public BuildRepository() : base(new BuildComparer())
        {
        }
        
        public Build GetBuild(int buildId)
        {
            return base.Get(() => data.FirstOrDefault(b => b.id == buildId));
        }

        public Build GetBuildBySourceVersion(string commitId)
        {
            return base.Get(() => data.Where(b => b.sourceVersion == commitId).OrderByDescending(b => b.id).FirstOrDefault());
        }
    }

    class BuildComparer : IEqualityComparer<Build>
    {
        public bool Equals(Build x, Build y)
        {
            return x.id == y.id;
        }

        public int GetHashCode(Build obj)
        {
            return obj.id;
        }
    }
}
