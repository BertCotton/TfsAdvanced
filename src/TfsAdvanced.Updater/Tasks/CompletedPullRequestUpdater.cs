using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
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

        protected override string GetPullRequestUrl(Repository repository)
        {
            return $"{repository.PullRequestUrl}?status=Completed";
        }
    }
}
