using Microsoft.Extensions.Configuration;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class ProjectRepository : SqlRepositoryBase<Project>
    {
        public ProjectRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public Project GetProject(string projectId)
        {
            return base.Get(p => p.ProjectId == projectId);
        }


        protected override bool Matches(Project source, Project target)
        {
            return source.Id == target.Id && source.ProjectId == target.ProjectId;
        }

        protected override void Map(Project @from, Project to)
        {
            to.Name = from.Name;
            to.Url = from.Url;
        }
    }
    
}
