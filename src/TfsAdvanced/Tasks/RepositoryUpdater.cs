using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Newtonsoft.Json;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Errors;
using TfsAdvanced.Data.Policy;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Repository;
using TfsAdvanced.Utilities;

namespace TfsAdvanced.Tasks
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
                ConcurrentBag<Data.Repositories.Repository> populatedRepositories = new ConcurrentBag<Data.Repositories.Repository>();
                Parallel.ForEach(projectRepository.GetProjects(), new ParallelOptions {MaxDegreeOfParallelism = Startup.MAX_DEGREE_OF_PARALLELISM}, project =>
                {
                    IList<Data.Repositories.Repository> repositories = GetAsync.FetchResponseList<Data.Repositories.Repository>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/git/repositories?api=1.0").Result;
                    if (repositories == null)
                        return;
                    Parallel.ForEach(repositories, new ParallelOptions {MaxDegreeOfParallelism = Startup.MAX_DEGREE_OF_PARALLELISM}, repo =>
                    {
                        var populatedRepository = GetAsync.Fetch<Data.Repositories.Repository>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/git/repositories/{repo.name}?api=1.0").Result;
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
