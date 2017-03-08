using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;

namespace TfsAdvanced.Updater.Tasks
{
    public class RepositoryUpdater
    {
        private readonly ProjectRepository projectRepository;
        private readonly RepositoryRepository repositoryRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly PolicyRepository policyRepository;
        private readonly RequestData requestData;
        private bool IsRunning;

        public RepositoryUpdater(ProjectRepository projectRepository, RequestData requestData, RepositoryRepository repositoryRepository, UpdateStatusRepository updateStatusRepository, PolicyRepository policyRepository)
        {
            this.projectRepository = projectRepository;
            this.requestData = requestData;
            this.repositoryRepository = repositoryRepository;
            this.updateStatusRepository = updateStatusRepository;
            this.policyRepository = policyRepository;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Update()
        {
            if (IsRunning)
                return;
            IsRunning = true;
            try
            {
                ConcurrentBag<TfsAdvanced.Models.Repositories.Repository> populatedRepositories = new ConcurrentBag<TfsAdvanced.Models.Repositories.Repository>();
                Parallel.ForEach(projectRepository.GetProjects(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, project =>
                {
                    IList<TfsAdvanced.Models.Repositories.Repository> repositories = GetAsync.FetchResponseList<TfsAdvanced.Models.Repositories.Repository>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/git/repositories?api=1.0").Result;
                    if (repositories == null)
                        return;
                    Parallel.ForEach(repositories, new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, repo =>
                    {
                        var populatedRepository = GetAsync.Fetch<TfsAdvanced.Models.Repositories.Repository>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/git/repositories/{repo.name}?api=1.0").Result;
                        populatedRepository.policyConfigurations = policyRepository.GetByRepository(populatedRepository.id);
                        populatedRepositories.Add(populatedRepository);
                    });
                });
                var repositoryList = populatedRepositories.ToList();
                repositoryRepository.UpdateRepositories(repositoryList);
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
