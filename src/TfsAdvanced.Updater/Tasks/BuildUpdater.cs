using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Logging;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Updater.Models.Builds;
using TFSAdvanced.Updater.Tasks;
using Build = TFSAdvanced.Models.DTO.Build;
using BuildStatus = TFSAdvanced.Models.DTO.BuildStatus;

namespace TfsAdvanced.Updater.Tasks
{
    public class BuildUpdater : UpdaterBase
    {
        private readonly BuildRepository buildRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly ProjectRepository projectRepository;
        private readonly RequestData requestData;
        private readonly RepositoryRepository repositoryRepository;
        private readonly BuildDefinitionRepository buildDefinitionRepository;
        private DateTime lastUpdate;
    
        public BuildUpdater(BuildRepository buildRepository, RequestData requestData, ProjectRepository projectRepository, UpdateStatusRepository updateStatusRepository,
            RepositoryRepository repositoryRepository, BuildDefinitionRepository buildDefinitionRepository, ILogger<BuildUpdater> logger) :base(logger)
        {
            this.buildRepository = buildRepository;
            this.requestData = requestData;
            this.projectRepository = projectRepository;
            this.updateStatusRepository = updateStatusRepository;
            this.repositoryRepository = repositoryRepository;
            this.buildDefinitionRepository = buildDefinitionRepository;
        }

        protected override async Task Update(bool initialize)
        {
            if (initialize && !buildRepository.IsEmpty())
                return;

            List<Build> builds = new List<Build>();
            foreach (var project in projectRepository.GetAll())
            {

                // Current active builds
                var projectBuilds = await GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.Builds.Build>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/build/builds?api-version=2.2&statusFilter=inProgress&statusFilter=notStarted");
                builds.AddRange(projectBuilds.Select(CreateBuild));

                DateTime requestWindow = DateTime.Now.AddDays(-3);
                if (lastUpdate > DateTime.MinValue)
                    requestWindow = lastUpdate.AddMinutes(-5);

                projectBuilds = await GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.Builds.Build>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/build/builds?api-version=2.2&minFinishTime={requestWindow:O}");
                builds.AddRange(projectBuilds.Select(CreateBuild));
            }

            await buildRepository.Update(builds);
            lastUpdate = DateTime.Now;

            // The builds must be requested without the filter because the only filter available is minFinishTime, which will filter out those that haven't finished yet
            updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = builds.Count, UpdaterName = nameof(BuildUpdater)});
        }
        

        private Build CreateBuild(TFSAdvanced.Updater.Models.Builds.Build build)
        {
            Build buildDto = new Build
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
                    UniqueName = build.requestedFor.uniqueName,
                    IconUrl = build.requestedFor.imageUrl,
                    Name = build.requestedFor.displayName
                }
            };

            Repository repository = null;

            if (build.definition != null)
            {
                if (build.definition.repository == null)
                {
                    var buildDefinitionDto = buildDefinitionRepository.GetBuildDefinition(build.definition.id);
                    if(buildDefinitionDto != null)
                        buildDto.Repository = buildDefinitionDto.Repository;
                }
                else
                {
                    buildDto.Repository = repositoryRepository.GetById(build.definition.repository.id);
                }
            }


            switch (build.status)
            {
                case TFSAdvanced.Updater.Models.Builds.BuildStatus.notStarted:
                    buildDto.BuildStatus = BuildStatus.NotStarted;
                    break;
                case TFSAdvanced.Updater.Models.Builds.BuildStatus.inProgress:
                    buildDto.BuildStatus = BuildStatus.Building;
                    break;
                default:
                    switch (build.result)
                    {
                        case BuildResult.abandoned:
                            buildDto.BuildStatus = BuildStatus.Abandonded;
                            break;
                        case BuildResult.canceled:
                            buildDto.BuildStatus = BuildStatus.Cancelled;
                            break;
                        case BuildResult.expired:
                            buildDto.BuildStatus = BuildStatus.Expired;
                            break;
                        case BuildResult.failed:
                        case BuildResult.partiallySucceeded:
                            buildDto.BuildStatus = BuildStatus.Failed;
                            break;
                        case BuildResult.succeeded:
                            buildDto.BuildStatus = BuildStatus.Succeeded;
                            break;
                        default:
                            buildDto.BuildStatus = BuildStatus.NoBuild;
                            break;
                    }
                    break;
            }

            return buildDto;
        }
    }
}
