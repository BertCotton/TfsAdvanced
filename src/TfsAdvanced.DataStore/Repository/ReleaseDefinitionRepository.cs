using TFSAdvanced.Models.DTO;

namespace TFSAdvanced.DataStore.Repository
{
    public class ReleaseDefinitionRepository : RepositoryBase<ReleaseDefinition>
    {
        protected override int GetId(ReleaseDefinition item)
        {
            return item.Id;
        }

        public ReleaseDefinition GetReleaseDefinition(string projectId, int id)
        {
            return Get(definition => definition.Project.Id == projectId && definition.Id == id);
        }
    }
}
