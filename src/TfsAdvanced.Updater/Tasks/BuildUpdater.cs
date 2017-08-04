using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Updater.Models.Builds;
using Build = TFSAdvanced.Updater.Models.Builds.Build;
using BuildStatus = TFSAdvanced.Updater.Models.Builds.BuildStatus;

namespace TfsAdvanced.Updater.Tasks
{
    public class BuildUpdater
    {
        private readonly BuildRepository buildRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly ProjectRepository projectRepository;
        private readonly RequestData requestData;
        private bool IsRunning;

        public BuildUpdater(BuildRepository buildRepository, RequestData requestData, ProjectRepository projectRepository, UpdateStatusRepository updateStatusRepository)
        {
            this.buildRepository = buildRepository;
            this.requestData = requestData;
            this.projectRepository = projectRepository;
            this.updateStatusRepository = updateStatusRepository;
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
                    var projectBuilds = GetAsync.FetchResponseList<Build>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/build/builds?api-version=2.2&reasonFilter=validateShelveset&minFinishTime={yesterday:O}").Result;
                    if (projectBuilds != null && projectBuilds.Any())
                    {
                        builds.PushRange(projectBuilds.ToArray());
                    }


                    // Current active builds
                    projectBuilds = GetAsync.FetchResponseList<Build>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/build/builds?api-version=2.2&statusFilter=inProgress&inProgress=notStarted").Result;
                    if (projectBuilds != null && projectBuilds.Any())
                    {
                        builds.PushRange(projectBuilds.ToArray());
                    }


                    DateTime twoHoursAgo = DateTime.Now.AddHours(-2);
                    // Because we want to capture the final state of any build that was running and just finished we are getting those too
                    // Finished builds within the last 2 hours
                    projectBuilds = GetAsync.FetchResponseList<Build>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/build/builds?api-version=2.2&minFinishTime={twoHoursAgo:O}").Result;
                    if (projectBuilds != null && projectBuilds.Any())
                    {
                        builds.PushRange(projectBuilds.ToArray());
                    }

                });


                // The builds must be requested without the filter because the only filter available is minFinishTime, which will filter out those that haven't finished yet
                var buildLists = builds.ToList();
                buildRepository.Update(buildLists.Select(CreateBuild));
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

        private TFSAdvanced.Models.DTO.Build CreateBuild(Build build)
        {
            TFSAdvanced.Models.DTO.Build buildDto = new TFSAdvanced.Models.DTO.Build
            {
                Id = build.id,
                Name = build.definition.name,
                Folder = build.definition.path,
                Url = build._links.web.href,
                SourceCommit = build.sourceVersion,
                QueuedDate = build.queueTime,
                StartedDate = build.startTime,
                FinishedDate = build.finishTime,
                Creator = new User
                {
                    Name = build.requestedFor.displayName,
                    IconUrl = build.requestedFor.imageUrl
                }
            };

            switch (build.status)
            {
                case BuildStatus.notStarted:
                    buildDto.BuildStatus = TFSAdvanced.Models.DTO.BuildStatus.NotStarted;
                    break;
                case BuildStatus.inProgress:
                    buildDto.BuildStatus = TFSAdvanced.Models.DTO.BuildStatus.Building;
                    break;
                default:
                    switch (build.result)
                    {
                        case BuildResult.abandoned:
                            buildDto.BuildStatus = TFSAdvanced.Models.DTO.BuildStatus.Abandonded;
                            break;
                        case BuildResult.canceled:
                            buildDto.BuildStatus = TFSAdvanced.Models.DTO.BuildStatus.Cancelled;
                            break;
                        case BuildResult.expired:
                            buildDto.BuildStatus = TFSAdvanced.Models.DTO.BuildStatus.Expired;
                            break;
                        case BuildResult.failed:
                        case BuildResult.partiallySucceeded:
                            buildDto.BuildStatus = TFSAdvanced.Models.DTO.BuildStatus.Failed;
                            break;
                        case BuildResult.succeeded:
                            buildDto.BuildStatus = TFSAdvanced.Models.DTO.BuildStatus.Succeeded;
                            break;
                        default:
                            buildDto.BuildStatus = TFSAdvanced.Models.DTO.BuildStatus.NoBuild;
                            break;
                    }
                    break;
            }

            return buildDto;
        }
    }
}
