using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Updater;
using TfsAdvanced.Updater.Tasks;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;
using PullRequest = TFSAdvanced.Updater.Models.PullRequests.PullRequest;

namespace TFSAdvanced.Updater.Tasks
{
    public class CompletedPullRequestUpdater : PullRequestUpdaterBase
    {
        public CompletedPullRequestUpdater(CompletedPullRequestRepository pullRequestRepository, RequestData requestData, RepositoryRepository repositoryRepository, UpdateStatusRepository updateStatusRepository, BuildRepository buildRepository, ILogger<PullRequestUpdater> logger) : base(pullRequestRepository, requestData, repositoryRepository, updateStatusRepository, buildRepository, logger)
        {
        }

        protected override IList<PullRequest> GetPullRequests(Repository repository)
        {
            var limit = 20;
            var skip = 0;
            var pullRequests = new List<PullRequest>();
            DateTime past2Days = DateTime.Now.Date.AddDays(-2);
            do
            {
                Logger.LogDebug($"Fetching the top {limit} (skipping {skip}) pull requests for {repository.Name}.");
                List<PullRequest> updatedPullRequests = GetAsync.FetchResponseList<PullRequest>(RequestData, repository.PullRequestUrl + $"?status=Completed&$top={limit}&$skip={skip}", Logger).Result;
                if (updatedPullRequests == null)
                    return pullRequests;

                // Only show the completed pull requests that have been completed in the past two days
                List<PullRequest> filteredPullRequests = updatedPullRequests.Where(x => x.creationDate >= past2Days).ToList();
                if (!filteredPullRequests.Any())
                {
                    Logger.LogDebug($"Finished fetching {pullRequests.Count} pull requests for repository {repository.Name}.");
                    return pullRequests;
                }

                pullRequests.AddRange(filteredPullRequests);
                skip += limit;
            } while (true);
        }
    }
}