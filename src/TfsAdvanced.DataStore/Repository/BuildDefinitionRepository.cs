using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TfsAdvanced.Models.Builds;

namespace TfsAdvanced.DataStore.Repository
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

        public BuildDefinition GetBuildDefinition(int definitionId)
        {
            return buildDefinitions.FirstOrDefault(x => x.id == definitionId);
        }

        public void Update(IList<BuildDefinition> buildDefinitions)
        {
            this.buildDefinitions = new ConcurrentBag<BuildDefinition>(buildDefinitions);
        }
    }
}
