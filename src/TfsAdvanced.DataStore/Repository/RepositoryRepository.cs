using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TFSAdvanced.DataStore;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class RepositoryRepository : SqlRepositoryBase<TFSAdvanced.Models.DTO.Repository>
    {
        public RepositoryRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public TFSAdvanced.Models.DTO.Repository GetById(string Id)
        {
            return Get(x => x.RepositoryId == Id);
        }

        protected override bool Matches(TFSAdvanced.Models.DTO.Repository source, TFSAdvanced.Models.DTO.Repository target)
        {
            return source.RepositoryId == target.RepositoryId;
        }

        protected override IQueryable<TFSAdvanced.Models.DTO.Repository> AddIncludes(DbSet<TFSAdvanced.Models.DTO.Repository> data)
        {
            return data.Include(repository => repository.Project);
        }

        public override void AttachProperties(TfsAdvancedSqlDataContext dataContext, TFSAdvanced.Models.DTO.Repository repository)
        {
            repository.Project = dataContext.Projects.FirstOrDefault(local => local.ProjectId == repository.Project.ProjectId);

        }

        protected override void Map(TFSAdvanced.Models.DTO.Repository @from, TFSAdvanced.Models.DTO.Repository to)
        {
            to.MinimumApproverCount = from.MinimumApproverCount;
            to.Name = from.Name;
        }
    }

}
