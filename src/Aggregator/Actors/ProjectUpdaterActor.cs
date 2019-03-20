using System;
using System.Threading;
using System.Threading.Tasks;
using Aggregator.Messages;
using Akka.Actor;
using Akka.Cluster.Tools.PublishSubscribe;
using Akka.DI.Core;
using Microsoft.Extensions.Logging;
using TfsAdvanced.Models;
using TFSAdvanced.Models.DTO;

namespace TFSAdvancedAggregator.Actors
{
    public class ProjectUpdaterActor : TFSBroadcastActorBase<ProjectUpdaterWorkerActor, Project>
    {
        public ProjectUpdaterActor(ILogger<ProjectUpdaterActor> logger) : base(logger, 1, nameof(SimpleMessages.UPDATE_PROJECTS))
        {
        }
    }
    public class ProjectUpdaterWorkerActor : TFSActorBase
    {
        private readonly RequestData requestData;

        public ProjectUpdaterWorkerActor(RequestData requestData, ILogger<ProjectUpdaterWorkerActor> logger) : base(logger)
        {
            this.requestData = requestData;

            ReceiveAsync<SimpleMessages.UPDATE_PROJECTS>(HandleUpdateProjectMessage);
        }

        private async Task HandleUpdateProjectMessage(SimpleMessages.UPDATE_PROJECTS message)
        { 
            LogInformation("Updating Projects");
            var projects = await GetAsync.FetchResponseList<Project>(requestData, $"{requestData.BaseAddress}/_apis/projects?api-version=1.0", logger);

            if (projects != null)
            {
                    LogInformation($"Publishing {projects.Count} Project Update Results");
                    foreach (var project in projects)
                    {
                        LogDebug($"Publishing Project Update For {project.Name}");
                        Sender.Tell(project, Self);
                        BroadcastMessage(new ProjectUpdatedMessage(project));
                    }
            }
            else
            {
                logger.LogInformation("No Projects found");
            }
            
        }
        
    }
}