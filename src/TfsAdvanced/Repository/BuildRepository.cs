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
            return builds.Where(b => b.definition.id == buildDefinition.id && b.sourceBranch == buildDefinition.repository.defaultBranch).OrderByDescending(b => b.id).Take(limit).OrderBy(b =>b.id).ToList();
        }

        public IList<Build> GetLatestBuildOnAllBranches(BuildDefinition buildDefinition, int limit)
        {
            return builds.Where(b => b.definition.id == buildDefinition.id).OrderByDescending(b => b.id).Take(limit).OrderBy(b => b.id).ToList();
        }

        public void Update(IList<Build> builds)
        {
            this.builds = new ConcurrentBag<Build>(builds);
        }


        public Build GetBuild(int buildId)
        {
            return this.builds.FirstOrDefault(b => b.id == buildId);
        }

        public Build GetBuildBySourceVersion(string commitId)
        {
            return this.builds.Where(b => b.sourceVersion == commitId).OrderByDescending(b => b.id).FirstOrDefault();
        }
    }
}
