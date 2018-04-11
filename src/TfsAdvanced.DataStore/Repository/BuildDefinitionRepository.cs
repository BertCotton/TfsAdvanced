using System;
using Redbus.Interfaces;
using TFSAdvanced.DataStore.Messages;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class BuildDefinitionRepository : RepositoryBase<BuildDefinition, BuildDefinitionUpdateMessage>
    {
        public BuildDefinitionRepository(IServiceProvider serviceProvider, IEventBus eventBus) : base(serviceProvider, eventBus)
        {
        }

        public BuildDefinition GetBuildDefinition(int definitionId)
        {
            return base.Get(definition => definition.Id == definitionId);
        }

        protected override int GetId(BuildDefinition item)
        {
            return item.Id;
        }
    }
}