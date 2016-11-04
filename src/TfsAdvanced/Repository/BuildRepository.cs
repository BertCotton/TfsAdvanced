using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;

namespace TfsAdvanced.Repository
{
    public class BuildRepository
    {
        private ConcurrentBag<Build> builds;

        public BuildRepository()
        {
            this.builds = new ConcurrentBag<Build>();
        }

        public IList<Build> GetBuilds()
        {
            return builds.ToList();
        }

        public IList<Build> GetBuilds(BuildDefinition buildDefinition)
        {
            return builds.Where(b => b.definition.id == buildDefinition.id).ToList();
        }

        public IList<Build> GetLatestBuildOnDefaultBranch(BuildDefinition buildDefinition, int limit)
        {
            return builds.Where(b => b.definition.id == buildDefinition.id && b.sourceBranch == buildDefinition.defaultBranch).OrderByDescending(b => b.id).Take(limit).OrderBy(b =>b.id).ToList();

        }

        public void Update(IList<Build> builds)
        {
            this.builds = new ConcurrentBag<Build>(builds);
        }


    }
}
