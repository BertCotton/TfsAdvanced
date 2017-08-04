using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Updater.Models.Policy;
using Repository = TFSAdvanced.Updater.Models.Repositories.Repository;

namespace TfsAdvanced.Updater.Tasks
{
    public class RepositoryUpdater
    {
        private readonly ProjectRepository projectRepository;
        private readonly RepositoryRepository repositoryRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly RequestData requestData;
        private bool IsRunning;

        public RepositoryUpdater(ProjectRepository projectRepository, RequestData requestData, RepositoryRepository repositoryRepository,
            UpdateStatusRepository updateStatusRepository)
        {
            this.projectRepository = projectRepository;
            this.requestData = requestData;
            this.repositoryRepository = repositoryRepository;
            this.updateStatusRepository = updateStatusRepository;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Update()
        {
            if (IsRunning)
                return;
            IsRunning = true;
            try
            {
                ConcurrentBag<TFSAdvanced.Models.DTO.Repository> populatedRepositories = new ConcurrentBag<TFSAdvanced.Models.DTO.Repository>();
                Parallel.ForEach(projectRepository.GetAll(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, project =>
                {
                    IList<Repository> repositories = GetAsync.FetchResponseList<Repository>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/git/repositories?api=1.0").Result;
                    if (repositories == null)
                        return;
                    Parallel.ForEach(repositories, new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, repo =>
                    {
                        try
                        {
                            var populatedRepository = GetAsync.Fetch<Repository>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/git/repositories/{repo.name}?api=1.0").Result;

                            var repositoryDto = new TFSAdvanced.Models.DTO.Repository
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
                        catch (Exception){}
                    });
                });
                var repositoryList = populatedRepositories.ToList();
                repositoryRepository.Update(repositoryList);
                updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = repositoryList.Count, UpdaterName = nameof(RepositoryUpdater)});
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error running repository updater", ex);
            }
            finally
            {
                IsRunning = false;
            }
        }
    }
}
