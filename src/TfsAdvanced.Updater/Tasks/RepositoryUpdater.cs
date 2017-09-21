using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        protected override async Task Update(bool initialize)
        {
            if (initialize && !repositoryRepository.IsEmpty())
                return;

            IDictionary<string, IList<PolicyConfiguration>> policyConfigurations = new Dictionary<string, IList<PolicyConfiguration>>();
            IList<Repository> repositories = new List<Repository>();
            foreach (var project in projectRepository.GetAll())
            {
                foreach (var repository in await GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.Repositories.Repository>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/git/repositories?api=1.0"))
                {

                    var populatedRepository = await GetAsync.Fetch<TFSAdvanced.Updater.Models.Repositories.Repository>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/git/repositories/{repository.name}?api=1.0");

                    var repositoryDto = new Repository
                    {
                        RepositoryId = populatedRepository.id,
                        Name = populatedRepository.name,
                        PullRequestUrl = populatedRepository._links.pullRequests.href,
                        Url = populatedRepository.remoteUrl,
                        Project = new Project
                        {
                            ProjectId = repository.project.id
                        }
                    };
                    IList<PolicyConfiguration> policyConfiguration;
                    if (!policyConfigurations.TryGetValue(project.ProjectId, out policyConfiguration))
                    {
                        policyConfiguration = (await GetAsync.FetchResponseList<PolicyConfiguration>(requestData, $"{requestData.BaseAddress}/defaultcollection/{project.ProjectId}/_apis/policy/configurations?api-version=2.0-preview.1")).ToList();
                        policyConfigurations.Add(project.ProjectId, policyConfiguration);
                    }

                    foreach (var configuration in policyConfiguration)
                    {
                        if (configuration.type.displayName == "Minimum number of reviewers")
                        {
                            repositoryDto.MinimumApproverCount = configuration.settings.minimumApproverCount;
                        }
                    }
                    repositories.Add(repositoryDto);
                }
            }
            await repositoryRepository.Update(repositories);
            updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = repositories.Count, UpdaterName = nameof(RepositoryUpdater)});
        }

      
    }
}
