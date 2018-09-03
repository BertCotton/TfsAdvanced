using System;
using Redbus.Interfaces;
using TFSAdvanced.DataStore.Messages;
using TFSAdvanced.Models.DTO;

namespace TFSAdvanced.DataStore.Repository
{
    public class ReleaseDefinitionRepository : RepositoryBase<ReleaseDefinition, ReleaseDefinitionUpdateMessage>
    {
        public ReleaseDefinitionRepository(IServiceProvider serviceProvider, IEventBus eventBus) : base(serviceProvider, eventBus)
        {
        }

        protected override int GetId(ReleaseDefinition item)
        {
            return item.Id;
        }

        public ReleaseDefinition GetReleaseDefinition(string projectId, int id)
        {
            return Get(definition => definition.Project?.Id == projectId && definition.Id == id);
        }
    }
}