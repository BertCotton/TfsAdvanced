using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class BuildDefinitionRepository : RepositoryBase<BuildDefinition>
    {

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
