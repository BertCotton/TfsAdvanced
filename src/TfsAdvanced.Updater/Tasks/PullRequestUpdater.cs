using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Builds;
using TfsAdvanced.Models.Infrastructure;
using TfsAdvanced.Models.PullRequests;

namespace TfsAdvanced.Updater.Tasks
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
                Parallel.ForEach(repositoryRepository.GetRepositories(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, repository =>
                {
                    if (repository._links.pullRequests == null)
                        return;
                    var pullRequests = GetAsync.FetchResponseList<PullRequest>(requestData, repository._links.pullRequests.href).Result;
                    if (pullRequests == null)
                        return;
                    Parallel.ForEach(pullRequests, new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, pullRequest =>
                    {
                        pullRequest.repository = repository;
                        pullRequest.remoteUrl = BuildPullRequestUrl(pullRequest, requestData.BaseAddress);
                        if (pullRequest.lastMergeCommit != null)
                        {
                            pullRequest.build = buildRepository.GetBuildBySourceVersion(pullRequest.lastMergeCommit.commitId);
                            //if (pullRequest.build?.finishTime != null && pullRequest.build.finishTime.Value.AddHours(12) < DateTime.Now)
                            //{
                            //    pullRequest.build.result = BuildResult.expired;
                            //}
                        }
                        foreach (var configuration in repository.policyConfigurations)
                        {
                            if (configuration.type.displayName == "Minimum number of reviewers")
                            {
                                pullRequest.requiredReviewers = configuration.settings.minimumApproverCount;
                            }
                        }
                        foreach (var reviewer in pullRequest.reviewers)
                        {
                            // Container reviewers do not count
                            if (reviewer.isContainer)
                                continue;
                            if (reviewer.vote == (int) Vote.Approved)
                                pullRequest.acceptedReviewers++;
                        }
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
