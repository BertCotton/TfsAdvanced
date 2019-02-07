using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TfsAdvanced.Updater;
using TfsAdvanced.Updater.Tasks;
using TFSAdvanced.Updater.Models.PullRequests;

namespace TFSAdvanced.Updater.Tasks
{
    public class PullRequestUpdater : PullRequestUpdaterBase
    {
        public PullRequestUpdater(PullRequestRepository pullRequestRepository, RequestData requestData, RepositoryRepository repositoryRepository, UpdateStatusRepository updateStatusRepository, BuildRepository buildRepository, ILogger<PullRequestUpdaterBase> logger) : base(pullRequestRepository, requestData, repositoryRepository, updateStatusRepository, buildRepository, logger)
        {
        }

        protected override void Update()
        {
            DateTime startTime = DateTime.Now;
            base.Update();
            IList<TFSAdvanced.Models.DTO.PullRequest> stalePullRequests = PullRequestRepository.GetStale(startTime);
            if (stalePullRequests.Any())
            {
                Parallel.ForEach(stalePullRequests, new ParallelOptions { MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM }, pullRequest =>
                {
                    PullRequest updatedPullRequest = GetAsync.Fetch<PullRequest>(RequestData, pullRequest.ApiUrl).Result;
                    if (updatedPullRequest.status != "active")
                        PullRequestRepository.Remove(new[] { pullRequest });
                });
            }
        }
    }
}