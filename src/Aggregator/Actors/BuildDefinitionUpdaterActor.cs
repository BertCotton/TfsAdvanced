using System.Collections.Generic;
using System.Threading.Tasks;
using Aggregator.Messages;
using Microsoft.Extensions.Logging;
using TfsAdvanced.Models;
using TFSAdvanced.Models.DTO;
using RawBuildDefinition = TFSAdvanced.Aggregator.Raw.Models.Builds.BuildDefinition;

namespace TFSAdvancedAggregator.Actors
{
    public class BuildDefinitionUpdaterActor : TFSBroadcastActorBase<BuildDefinitionUpdaterWorkerActor, BuildDefinition>
    {
        public BuildDefinitionUpdaterActor(ILogger<BuildDefinitionUpdaterActor> logger) : base(logger, 2, nameof(ProjectUpdatedMessage))
        {
        }
    }
    public class BuildDefinitionUpdaterWorkerActor : TFSActorBase
    {
        private readonly RequestData requestData;

        public BuildDefinitionUpdaterWorkerActor(RequestData requestData, ILogger<BuildDefinitionUpdaterWorkerActor> logger) : base(logger)
        {
            this.requestData = requestData;

            ReceiveAsync<ProjectUpdatedMessage>(message => HandleMessageAsync(message, HandleRepositoryUpdatedMessage));

            Receive<BuildDefinitionLightMessage>(message => HandleMessage(message, HandleBuildDefinitionLightMessage));
        }

        private async Task HandleRepositoryUpdatedMessage(ProjectUpdatedMessage message)
        {
            List<RawBuildDefinition> definitions = await GetAsync.FetchResponseList<RawBuildDefinition>(requestData, $"{requestData.BaseAddress}/{message.Project.Name}/_apis/build/definitions?api=2.2", logger);
            if (definitions == null)
            {
                LogInformation($"Unable to get the definition for the project {message.Project.Name}");
                return;
            }

            LogInformation($"Queueing {definitions.Count} Build Definitions to be loaded for {message.Project.Name}");
            foreach (var buildDefinition in definitions)
            {
                Self.Tell(new BuildDefinitionLightMessage(message.Project, buildDefinition), Sender);
            }
        }

        private bool HandleBuildDefinitionLightMessage(BuildDefinitionLightMessage message)
        {
            LogDebug($"Loading build definition {message.RawBuildDefinition.name} for repository in project {message.Project.Name}");
            RawBuildDefinition populatedDefinition = GetAsync.Fetch<RawBuildDefinition>(requestData, message.RawBuildDefinition.url).Result;
            
            var buildDefinitionDTO = new BuildDefinition
            {
                DefaultBranch = populatedDefinition.repository.defaultBranch,
                Folder = populatedDefinition.path,
                Id = populatedDefinition.id,
                Name = populatedDefinition.name,
                Url = populatedDefinition._links.web.href,
                Repository = new Repository
                {
                    Id = populatedDefinition.repository.id,
                    Name = populatedDefinition.repository.name,
                    Url = populatedDefinition.repository.url,
                    Project = message.Project
                } 
            };

            Sender.Tell(buildDefinitionDTO, Self);
            base.BroadcastMessage(new BuildDefinitionUpdatedMessage(buildDefinitionDTO));
            return true;
        }
        
    }

    internal class BuildDefinitionLightMessage : MessageBase
    {
        public BuildDefinitionLightMessage(Project project, RawBuildDefinition rawBuildDefinition)
        {
            Project = project;
            RawBuildDefinition = rawBuildDefinition;
        }

        public Project Project { get; set; }
        
        public RawBuildDefinition RawBuildDefinition { get; set; }
    }
}