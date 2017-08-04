using System.Collections.Generic;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class ProjectRepository : RepositoryBase<Project>
    {

        public Project GetProject(string projectId)
        {
            return base.Get(p => p.Id == projectId);
        }

        protected override int GetId(Project item)
        {
            return item.Id.GetHashCode();
        }
    }
    
}
