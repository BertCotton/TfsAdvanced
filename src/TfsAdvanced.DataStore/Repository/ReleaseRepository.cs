using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TFSAdvanced.Models.DTO;

namespace TFSAdvanced.DataStore.Repository
{
    public class ReleaseRepository : SqlRepositoryBase<Release>
    {
        public ReleaseRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override bool Matches(Release source, Release target)
        {
            return source.Id == target.Id;
        }

        public Release GetById(int id)
        {
            return Get(release => release.Id == id);
        }

        public override void AttachProperties(TfsAdvancedSqlDataContext dataContext, Release release)
        {
            release.Project = dataContext.Projects.FirstOrDefault(x => x.ProjectId == release.Project.ProjectId);
            release.CreatedBy = GetOrUpdate(dataContext, x => x.UniqueName == release.CreatedBy.UniqueName, release.CreatedBy);
                
            release.ReleaseDefinition = dataContext.ReleaseDefinitions.FirstOrDefault(x => x.Id == release.ReleaseDefinition.Id);
        }

        protected override IQueryable<Release> AddIncludes(DbSet<Release> data)
        {
            return data.Include(x => x.CreatedBy)
                    .Include(x => x.Project)
                    .Include(x => x.ReleaseDefinition).ThenInclude(x => x.Project)
                ;
        }

        protected override void Map(Release @from, Release to)
        {
        }
    }
}
