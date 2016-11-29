using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
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
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly BuildRepository buildRepository;
        private bool IsRunning;

        public PullRequestUpdater(PullRequestRepository pullRequestRepository, RequestData requestData, RepositoryRepository repositoryRepository, UpdateStatusRepository updateStatusRepository, BuildRepository buildRepository)
        {
            this.requestData = requestData;
            this.repositoryRepository = repositoryRepository;
            this.updateStatusRepository = updateStatusRepository;
            this.buildRepository = buildRepository;
            this.pullRequestRepository = pullRequestRepository;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Update()
        {
            if (IsRunning)
                return;

            IsRunning = true;
            try
            {
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
                        if(pullRequest.lastMergeCommit != null)
                            pullRequest.build = buildRepository.GetBuildBySourceVersion(pullRequest.lastMergeCommit.commitId);
                        allPullRequests.Add(pullRequest);
                    });
                });
                var pullRequestsList = allPullRequests.ToList();
                pullRequestRepository.UpdatePullRequests(pullRequestsList);
                updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = pullRequestsList.Count, UpdaterName = nameof(PullRequestUpdater)});


            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error runnign Pull Request Updater", ex);
            }
            finally
            {
                IsRunning = false;
            }
        }

        public string BuildPullRequestUrl(PullRequest pullRequest, string baseUrl)
        {
            return
                $"{baseUrl}/{pullRequest.repository.project.name}/_git/{pullRequest.repository.name}/pullrequest/{pullRequest.pullRequestId}?view=files";
        }
    }
}
