using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Updater.Tasks;
using TFSAdvanced.DataStore.Interfaces;

namespace TFSAdvanced.Updater.Tasks
{
    public class PullRequestUpdater : PullRequestUpdaterBase
    {
        public PullRequestUpdater(PullRequestRepository pullRequestRepository, RequestData requestData, RepositoryRepository repositoryRepository, UpdateStatusRepository updateStatusRepository, BuildRepository buildRepository, ILogger<PullRequestUpdaterBase> logger) : base(pullRequestRepository, requestData, repositoryRepository, updateStatusRepository, buildRepository, logger)
        {
        }
    }
}
