using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Builds;
using TfsAdvanced.Models.Infrastructure;

namespace TfsAdvanced.Updater.Tasks
{
    public class BuildUpdater
    {
        private readonly BuildRepository buildRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly ProjectRepository projectRepository;
        private readonly PullRequestRepository pullRequestRepository;
        private readonly RequestData requestData;
        private bool IsRunning;

        public BuildUpdater(BuildRepository buildRepository, RequestData requestData, ProjectRepository projectRepository, UpdateStatusRepository updateStatusRepository, PullRequestRepository pullRequestRepository)
        {
            this.buildRepository = buildRepository;
            this.requestData = requestData;
            this.projectRepository = projectRepository;
            this.updateStatusRepository = updateStatusRepository;
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

                DateTime yesterday = DateTime.Now.Date.AddDays(-1);
                var builds = new ConcurrentStack<Build>();
                Parallel.ForEach(projectRepository.GetAll(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, project =>
                {
                    // Finished PR builds                    
                    var projectBuilds = GetAsync.FetchResponseList<Build>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/build/builds?api-version=2.2&reasonFilter=validateShelveset&minFinishTime={yesterday:O}").Result;
                    if (projectBuilds != null && projectBuilds.Any())
                    {
                        builds.PushRange(projectBuilds.ToArray());
                    }


                    // Current active builds
                    projectBuilds = GetAsync.FetchResponseList<Build>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/build/builds?api-version=2.2&statusFilter=inProgress&inProgress=notStarted").Result;
                    if (projectBuilds != null && projectBuilds.Any())
                    {
                        builds.PushRange(projectBuilds.ToArray());
                    }


                    DateTime twoHoursAgo = DateTime.Now.AddHours(-2);
                    // Because we want to capture the final state of any build that was running and just finished we are getting those too
                    // Finished builds within the last 2 hours
                    projectBuilds = GetAsync.FetchResponseList<Build>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/build/builds?api-version=2.2&minFinishTime={twoHoursAgo:O}").Result;
                    if (projectBuilds != null && projectBuilds.Any())
                    {
                        builds.PushRange(projectBuilds.ToArray());
                    }

                });


                // The builds must be requested without the filter because the only filter available is minFinishTime, which will filter out those that haven't finished yet
                var buildLists = builds.ToList();
                buildRepository.Update(buildLists);
                updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = buildLists.Count, UpdaterName = nameof(BuildUpdater)});

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error running build updater", ex);
            }
            finally
            {
                IsRunning = false;
            }
        }
    }
}
