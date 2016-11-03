using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data.Builds;

namespace TfsAdvanced.Repository
{
    public class BuildDefinitionRepository
    {
        private ConcurrentBag<BuildDefinition> buildDefinitions;

        public BuildDefinitionRepository()
        {
            buildDefinitions = new ConcurrentBag<BuildDefinition>();
        }

        public IList<BuildDefinition> GetBuildDefinitions()
        {
            return buildDefinitions.ToList();
        }

        public void Update(IList<BuildDefinition> buildDefinitions)
        {
            this.buildDefinitions = new ConcurrentBag<BuildDefinition>(buildDefinitions);
        }
    }
}
