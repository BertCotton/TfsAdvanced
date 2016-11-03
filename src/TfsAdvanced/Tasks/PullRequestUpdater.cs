using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.PullRequests;
using TfsAdvanced.Repository;
using TfsAdvanced.Utilities;

namespace TfsAdvanced.Tasks
{
    public class PullRequestUpdater
    {
        private readonly RequestData requestData;
        private readonly PullRequestRepository pullRequestRepository;
        private readonly RepositoryRepository repositoryRepository;
        private bool IsRunning;

        public PullRequestUpdater(PullRequestRepository pullRequestRepository, RequestData requestData, RepositoryRepository repositoryRepository)
        {
            this.requestData = requestData;
            this.repositoryRepository = repositoryRepository;
            this.pullRequestRepository = pullRequestRepository;
        }

        public void Update()
        {
            if (IsRunning)
                return;

            IsRunning = true;

            ConcurrentBag<PullRequest> allPullRequests = new ConcurrentBag<PullRequest>();
            Parallel.ForEach(repositoryRepository.GetRepositories(), new ParallelOptions {MaxDegreeOfParallelism = Startup.MAX_DEGREE_OF_PARALLELISM}, repository =>
            {
                if (repository._links.pullRequests == null)
                    return;
                var pullRequests = GetAsync.FetchResponseList<PullRequest>(requestData, repository._links.pullRequests.href).Result;
                if (pullRequests == null)
                    return;
                Parallel.ForEach(pullRequests, new ParallelOptions {MaxDegreeOfParallelism = Startup.MAX_DEGREE_OF_PARALLELISM}, pullRequest =>
                {
                    pullRequest.repository = repository;
                    pullRequest.remoteUrl = BuildPullRequestUrl(pullRequest, requestData.BaseAddress);
                    allPullRequests.Add(pullRequest);
                });
            });
            pullRequestRepository.UpdatePullRequests(allPullRequests.ToList());

            IsRunning = false;
        }

        public string BuildPullRequestUrl(PullRequest pullRequest, string baseUrl)
        {
            return
                $"{baseUrl}/{pullRequest.repository.project.name}/_git/{pullRequest.repository.name}/pullrequest/{pullRequest.pullRequestId}?view=files";
        }
    }
}
