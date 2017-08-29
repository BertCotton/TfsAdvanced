using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;   
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Updater.Models.Policy;
using TFSAdvanced.Updater.Tasks;

namespace TfsAdvanced.Updater.Tasks
{
    public class RepositoryUpdater : UpdaterBase
    {
        private readonly ProjectRepository projectRepository;
        private readonly RepositoryRepository repositoryRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly RequestData requestData;
        
        public RepositoryUpdater(ProjectRepository projectRepository, RequestData requestData, RepositoryRepository repositoryRepository,
            UpdateStatusRepository updateStatusRepository, ILogger<RepositoryUpdater> logger) : base(logger)
        {
            this.projectRepository = projectRepository;
            this.requestData = requestData;
            this.repositoryRepository = repositoryRepository;
            this.updateStatusRepository = updateStatusRepository;
        }

        protected override void Update()
        {
            ConcurrentBag<Repository> populatedRepositories = new ConcurrentBag<Repository>();
            Parallel.ForEach(projectRepository.GetAll(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, project =>
            {
                IList<TFSAdvanced.Updater.Models.Repositories.Repository> repositories = GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.Repositories.Repository>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/git/repositories?api=1.0").Result;
                if (repositories == null)
                    return;
                Parallel.ForEach(repositories, new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, repo =>
                {
                    try
                    {
                        var populatedRepository = GetAsync.Fetch<TFSAdvanced.Updater.Models.Repositories.Repository>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/git/repositories/{repo.name}?api=1.0").Result;

                        var repositoryDto = new Repository
                        {
                            Id = populatedRepository.id,
                            Name = populatedRepository.name,
                            PullRequestUrl = populatedRepository._links.pullRequests.href,
                            Url = populatedRepository.remoteUrl,
                            Project = new Project
                            {
                                Id = populatedRepository.project.id,
                                Name = populatedRepository.project.name,
                                Url = populatedRepository.project.url
                            }
                        };
                        var policyConfigurations = GetAsync.FetchResponseList<PolicyConfiguration>(requestData, $"{requestData.BaseAddress}/defaultcollection/{project.Id}/_apis/policy/configurations?api-version=2.0-preview.1").Result;

                        foreach (var configuration in policyConfigurations)
                        {
                            if (configuration.type.displayName == "Minimum number of reviewers")
                            {
                                repositoryDto.MinimumApproverCount = configuration.settings.minimumApproverCount;
                            }
                        }
                        populatedRepositories.Add(repositoryDto);
                    }
                    catch (Exception)
                    {
                    }
                });
            });
            var repositoryList = populatedRepositories.ToList();
            repositoryRepository.Update(repositoryList);
            updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = repositoryList.Count, UpdaterName = nameof(RepositoryUpdater)});
        }

      
    }
}
