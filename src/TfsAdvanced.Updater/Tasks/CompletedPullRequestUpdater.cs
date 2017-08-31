using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Updater;
using TfsAdvanced.Updater.Tasks;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TFSAdvanced.Updater.Tasks
{
    public class CompletedPullRequestUpdater : PullRequestUpdaterBase
    {
        public CompletedPullRequestUpdater(CompletedPullRequestRepository pullRequestRepository, RequestData requestData, RepositoryRepository repositoryRepository, UpdateStatusRepository updateStatusRepository, BuildRepository buildRepository, ILogger<PullRequestUpdater> logger) : base(pullRequestRepository, requestData, repositoryRepository, updateStatusRepository, buildRepository, logger)
        {
        }

        protected override IList<TFSAdvanced.Updater.Models.PullRequests.PullRequest> GetPullRequests(Repository repository)
        {
            var pullRequests = GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.PullRequests.PullRequest>(requestData, repository.PullRequestUrl + "?status=Completed").Result;
            if (pullRequests == null)
                return null;
            var past2Days = DateTime.Now.Date.AddDays(-2);
            // Only show the completed pull requests that have been completed in the past two days
            return pullRequests.Where(x => x.creationDate >= past2Days).ToList();
        }

    }
}
