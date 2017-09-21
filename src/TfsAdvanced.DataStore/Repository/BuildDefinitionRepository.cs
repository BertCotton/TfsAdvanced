using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using TFSAdvanced.DataStore;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class BuildDefinitionRepository : SqlRepositoryBase<BuildDefinition>
    {

        public BuildDefinitionRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public BuildDefinition GetBuildDefinition(int definitionId)
        {
            return base.Get(definition => definition.Id == definitionId);
        }

        protected override bool Matches(BuildDefinition source, BuildDefinition target)
        {
            return source.Id == target.Id && source.Repository?.RepositoryId == target.Repository?.RepositoryId;
        }

        protected override IQueryable<BuildDefinition> AddIncludes(DbSet<BuildDefinition> data)
        {
            return data.Include(definition => definition.Repository).ThenInclude(repository => repository.Project);
        }

        public override void AttachProperties(TfsAdvancedSqlDataContext dataContext, BuildDefinition build)
        {
            if(build.Repository != null)
                build.Repository = dataContext.Repositories.FirstOrDefault(x => x.RepositoryId == build.Repository.RepositoryId);
        }

        protected override void Map(BuildDefinition from, BuildDefinition to)
        {
            
        }

        
    }
    
}
