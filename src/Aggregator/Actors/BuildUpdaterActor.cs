using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aggregator.Messages;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Microsoft.Extensions.Logging;
using TfsAdvanced.Models;
using TFSAdvanced.Models.DTO;
using Build = TFSAdvanced.Models.DTO.Build;
using RawBuild = TFSAdvanced.Aggregator.Raw.Models.Builds.Build;
using RawBuildStatus = TFSAdvanced.Aggregator.Raw.Models.Builds.BuildStatus;
using RawBuildResult = TFSAdvanced.Aggregator.Raw.Models.Builds.BuildResult;

namespace TFSAdvancedAggregator.Actors
{
    public class BuildUpdaterActor : TFSBroadcastActorBase<BuildUpdaterWorkerActor, Build>
    {
        public BuildUpdaterActor(ILogger<BuildUpdaterActor> logger) : base(logger, 10, nameof(BuildDefinitionUpdatedMessage))
        {
        }
    }

    public class BuildUpdaterWorkerActor : TFSActorBase
    {
        private readonly RequestData requestData;
        private DateTime lastRequest;

        public BuildUpdaterWorkerActor(RequestData requestData, ILogger<BuildUpdaterWorkerActor> logger) : base(logger)
        {
            lastRequest = DateTime.Now.AddDays(-3);
            this.requestData = requestData;

            
            ReceiveAsync<BuildDefinitionUpdatedMessage>(message => HandleMessageAsync(message, HandleBuildDefinitionUpdatedMessage));
        }

        private async Task HandleBuildDefinitionUpdatedMessage(BuildDefinitionUpdatedMessage message)
        {
            LogInformation($"Fetching Builds for Build Definiton {message.BuildDefinition.Name} in {message.BuildDefinition.Repository.Name}/{message.BuildDefinition.Repository.Project.Name}");
            // Finished PR builds
            List<RawBuild> projectBuilds = await GetAsync.FetchResponseList<RawBuild>(requestData, $"{requestData.BaseAddress}/{message.BuildDefinition.Repository.Project.Name}/_apis/build/builds?api-version=2.2&minFinishTime={lastRequest:O}", logger);
            if (projectBuilds != null && projectBuilds.Any())
            {
                foreach (var projectBuild in projectBuilds)
                {
                    var build = CreateBuild(projectBuild, message);
                    Sender.Tell(build, Self);
                    BroadcastMessage(new BuildUpdatedMessage(build));
                }
            }

            LogDebug($"Fetching active build updates for project {message.BuildDefinition.Repository.Project.Name}");
            // Current active builds
            projectBuilds = GetAsync.FetchResponseList<RawBuild>(requestData, $"{requestData.BaseAddress}/{message.BuildDefinition.Repository.Project.Name}/_apis/build/builds?api-version=2.2&statusFilter=inProgress&statusFilter=notStarted", logger).Result;
            if (projectBuilds != null && projectBuilds.Any())
            {
                foreach (var projectBuild in projectBuilds)
                {
                    var build = CreateBuild(projectBuild, message);
                    Sender.Tell(build, Self);
                    BroadcastMessage(new BuildUpdatedMessage(build));
                }
            }

            lastRequest = DateTime.Now;
        }

        private Build CreateBuild(RawBuild build, BuildDefinitionUpdatedMessage message)
        {
            var buildDto = new Build
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
                },
                Repository = message.BuildDefinition.Repository
            };


            switch (build.status)
            {
                case RawBuildStatus.notStarted:
                    buildDto.BuildStatus = BuildStatus.NotStarted;
                    break;

                case RawBuildStatus.inProgress:
                    buildDto.BuildStatus = BuildStatus.Building;
                    break;

                default:
                    switch (build.result)
                    {
                        case RawBuildResult.abandoned:
                            buildDto.BuildStatus = BuildStatus.Abandonded;
                            break;

                        case RawBuildResult.canceled:
                            buildDto.BuildStatus = BuildStatus.Cancelled;
                            break;

                        case RawBuildResult.expired:
                            buildDto.BuildStatus = BuildStatus.Expired;
                            break;

                        case RawBuildResult.failed:
                        case RawBuildResult.partiallySucceeded:
                            buildDto.BuildStatus = BuildStatus.Failed;
                            break;

                        case RawBuildResult.succeeded:
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