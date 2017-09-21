using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TFSAdvanced.DataStore;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class BuildRepository : SqlRepositoryBase<Build>
    {
        public BuildRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public Build GetBuild(int buildId)
        {
            return base.Get(b => b.Id == buildId);
        }

        public Build GetByDefinitionAndProject(int definitionId, string projectId)
        {
            return base.Get(build => build.Id == definitionId && build.Repository?.Project?.ProjectId == projectId);
        }

        public Build GetBuildBySourceVersion(TFSAdvanced.Models.DTO.Repository repository, string commitId)
        {
            var build = base.GetList(b => b.SourceCommit == commitId).OrderByDescending(b => b.Id).FirstOrDefault();
            if (build == null)
            {
                var repositoryBuilds = base.GetList(b => b.Repository?.RepositoryId == repository?.RepositoryId).ToList();
                foreach (var repositoryBuild in repositoryBuilds)
                {
                    if (repositoryBuild.SourceCommit == commitId)
                        build = repositoryBuild;
                }
            }
            return build;
        }

        protected override IQueryable<Build> AddIncludes(DbSet<Build> data)
        {
            return data.Include(build => build.Repository).ThenInclude(repository => repository.Project)
                .Include(build => build.Creator);
        }

        public override void AttachProperties(TfsAdvancedSqlDataContext dataContext, Build build)
        {
            if (build.Repository != null)
                build.Repository = dataContext.Repositories.FirstOrDefault(repository => repository.RepositoryId == build.Repository.RepositoryId);
            build.Creator = GetOrUpdate(dataContext, user => user.UniqueName == build.Creator.UniqueName, build.Creator);
        }

        protected override bool Matches(Build source, Build target)
        {
            return source.Id == target.Id;
        }

        protected override void Map(Build from, Build to)
        {
            to.BuildStatus = from.BuildStatus;
            to.FinishedDate = from.FinishedDate;
            to.QueuedDate = from.QueuedDate;
            to.SourceCommit = from.SourceCommit;
            to.StartedDate = from.StartedDate;
        }

        
    }
    
}
