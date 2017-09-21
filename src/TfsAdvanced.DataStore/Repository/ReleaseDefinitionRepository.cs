using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TFSAdvanced.Models.DTO;

namespace TFSAdvanced.DataStore.Repository
{
    public class ReleaseDefinitionRepository : SqlRepositoryBase<ReleaseDefinition>
    {
        public ReleaseDefinitionRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public ReleaseDefinition GetReleaseDefinition(string projectId, int releaseDefinitionId)
        {
            return Get(definition => definition.Project.ProjectId == projectId && definition.ReleaseDefinitionId == releaseDefinitionId);
        }

        protected override bool Matches(ReleaseDefinition source, ReleaseDefinition target)
        {
            return source.ReleaseDefinitionId == target.ReleaseDefinitionId && source.ProjectId == target.ProjectId;
        }

        protected override IQueryable<ReleaseDefinition> AddIncludes(DbSet<ReleaseDefinition> data)
        {
            return data.Include(definition => definition.Project);
        }

        public override void AttachProperties(TfsAdvancedSqlDataContext dataContext, ReleaseDefinition releaseDefinition)
        {
            releaseDefinition.Project = dataContext.Projects.FirstOrDefault(project => project.ProjectId == releaseDefinition.Project.ProjectId);

        }

        protected override void Map(ReleaseDefinition @from, ReleaseDefinition to)
        {
            to.Name = from.Name;
        }
    }
}
