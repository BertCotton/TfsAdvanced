using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Logging;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
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
        private DateTime lastRequest;

        public BuildUpdater(BuildRepository buildRepository, RequestData requestData, ProjectRepository projectRepository,
            UpdateStatusRepository updateStatusRepository,
            RepositoryRepository repositoryRepository, BuildDefinitionRepository buildDefinitionRepository,
            ILogger<BuildUpdater> logger)
            : base(logger)
        {
            this.buildRepository = buildRepository;
            this.requestData = requestData;
            this.projectRepository = projectRepository;
            this.updateStatusRepository = updateStatusRepository;
            this.repositoryRepository = repositoryRepository;
            this.buildDefinitionRepository = buildDefinitionRepository;
            lastRequest = DateTime.Now.AddDays(-3);
        }

        protected override void Update()
        {
            DateTime startTime = DateTime.Now;
            logger.LogInformation($"Fetching Build Updates Since {lastRequest}");
            var builds = new ConcurrentStack<TFSAdvanced.Updater.Models.Builds.Build>();
            Parallel.ForEach(projectRepository.GetAll(), new ParallelOptions { MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM }, project =>
              {
                  logger.LogDebug($"Fetching finished build updates for project {project.Name}");
                  // Finished PR builds
                  var projectBuilds = GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.Builds.Build>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/build/builds?api-version=2.2&minFinishTime={lastRequest:O}").Result;
                  if (projectBuilds != null && projectBuilds.Any())
                  {
                      builds.PushRange(projectBuilds.ToArray());
                  }

                  logger.LogDebug($"Fetching active build updates for project {project.Name}");
                  // Current active builds
                  projectBuilds = GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.Builds.Build>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/build/builds?api-version=2.2&statusFilter=inProgress&statusFilter=notStarted").Result;
                  if (projectBuilds != null && projectBuilds.Any())
                  {
                      builds.PushRange(projectBuilds.ToArray());
                  }
              });

            // The builds must be requested without the filter because the only filter available is minFinishTime, which will filter out those that haven't
            // finished yet
            var buildLists = builds.ToList();
            buildRepository.Update(buildLists.Select(CreateBuild));
            updateStatusRepository.UpdateStatus(new UpdateStatus { LastUpdate = DateTime.Now, UpdatedRecords = buildLists.Count, UpdaterName = nameof(BuildUpdater) });
            // Use the start time so that there is a small amount of overlap
            lastRequest = startTime;
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
                    Name = build.requestedFor.displayName,
                    IconUrl = build.requestedFor.imageUrl
                }
            };

            Repository repository = null;

            if (build.definition != null)
            {
                if (build.definition.repository == null)
                {
                    var buildDefinitionDto = buildDefinitionRepository.GetBuildDefinition(build.definition.id);
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