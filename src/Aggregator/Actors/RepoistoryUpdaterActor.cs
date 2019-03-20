using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aggregator.Messages;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using TfsAdvanced.Models;
using TFSAdvanced.Models.DTO;
using RawRepository = TFSAdvanced.Aggregator.Raw.Models.Repositories.Repository;

namespace TFSAdvancedAggregator.Actors
{
    public class RepositoryUpdaterActor : TFSBroadcastActorBase<RepositoryUpdaterWorkerActor, Repository>
    {
        public RepositoryUpdaterActor(ILogger<RepositoryUpdaterActor> logger) : base(logger, 5, nameof(ProjectUpdatedMessage))
        {
        }
    }
    
    public class RepositoryUpdaterWorkerActor : TFSActorBase
    {
        private readonly RequestData requestData;

        public RepositoryUpdaterWorkerActor(RequestData requestData, ILogger<RepositoryUpdaterActor> logger) : base(logger)
        {
            this.requestData = requestData;
            ReceiveAsync<ProjectUpdatedMessage>(async message =>
            {
                LogInformation($"Fetching repositories for project {message.Project.Name}");
                IList<RawRepository> repositories = await GetAsync.FetchResponseList<RawRepository>(requestData, $"{requestData.BaseAddress}/{message.Project.Name}/_apis/git/repositories?api=1.0", logger);
                if (repositories == null)
                {
                    LogInformation($"No repositories found for project {message.Project.Name}");
                    return;
                }

                foreach (var repository in repositories)
                {
                    Self.Tell(new RepositoryLightlyLoadedMessage(message.Project, repository), Sender);    
                }

                
            });

            ReceiveAsync<RepositoryLightlyLoadedMessage>(message => HandleMessageAsync(message, HydrateRepository));
        }

        private async Task HydrateRepository(RepositoryLightlyLoadedMessage message)
        {
            LogDebug($"Loading Repository {message.RawRepository.name}/{message.Project.Name}");
            
            RawRepository populatedRepository = await GetAsync.Fetch<RawRepository>(requestData, $"{requestData.BaseAddress}/{message.Project.Name}/_apis/git/repositories/{message.RawRepository.name}?api=1.0");

            var repositoryDto = new Repository
            {
                Id = populatedRepository.id,
                Name = populatedRepository.name,
                PullRequestUrl = populatedRepository._links.pullRequests.href,
                Url = populatedRepository.remoteUrl,
                Project = message.Project
            };

            Sender.Tell(repositoryDto, Self);
            base.BroadcastMessage(new RepositoryUpdatedMesssage(repositoryDto));
        }
    }

    public class RepositoryLightlyLoadedMessage : MessageBase
    {
        public Guid MessageID { get; set; }
        public Project Project { get; set; }

        public RawRepository RawRepository { get; set; }

        public RepositoryLightlyLoadedMessage(Project project, RawRepository rawRepository)
        {
            Project = project;
            RawRepository = rawRepository;
            MessageID = Guid.NewGuid();
        }
}
}