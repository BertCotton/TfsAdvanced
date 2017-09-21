using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Updater.Tasks;

namespace TfsAdvanced.Updater.Tasks
{
    public class BuildDefinitionUpdater : UpdaterBase
    {
        private readonly BuildDefinitionRepository buildDefinitionRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly ProjectRepository projectRepository;
        private readonly RepositoryRepository repositoryRepository;
        private readonly RequestData requestData;
        
        public BuildDefinitionUpdater(BuildDefinitionRepository buildDefinitionRepository, RequestData requestData, ProjectRepository projectRepository, UpdateStatusRepository updateStatusRepository, RepositoryRepository repositoryRepository, ILogger<BuildDefinitionUpdater> logger) :
            base(logger)
        {
            this.buildDefinitionRepository = buildDefinitionRepository;
            this.requestData = requestData;
            this.projectRepository = projectRepository;
            this.updateStatusRepository = updateStatusRepository;
            this.repositoryRepository = repositoryRepository;
        }

        protected override async Task Update(bool initialize)
        {
            if (initialize && !buildDefinitionRepository.IsEmpty())
                return;


            IList<BuildDefinition> buildDefinitions = new List<BuildDefinition>();
            foreach (var project in projectRepository.GetAll())
            {
                var definitions = await GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.Builds.BuildDefinition>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/build/definitions?api=2.2");
                if (definitions == null)
                {
                    logger.LogInformation($"Unable to get the definitiosn for the project {project.Name}");
                    return;
                }
                foreach (var definition in definitions)
                {
                    var populatedDefinition = await GetAsync.Fetch<TFSAdvanced.Updater.Models.Builds.BuildDefinition>(requestData, definition.url);
                    var repository = repositoryRepository.GetById(populatedDefinition.repository.id);

                    buildDefinitions.Add(new BuildDefinition
                    {
                        DefaultBranch = populatedDefinition.repository.defaultBranch,
                        Folder = populatedDefinition.path,
                        Id = populatedDefinition.id,
                        Name = populatedDefinition.name,
                        Url = populatedDefinition._links.web.href,
                        Repository = repository
                    });

                }
            }

            await buildDefinitionRepository.Update(buildDefinitions);

            updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = buildDefinitions.Count, UpdaterName = nameof(BuildDefinitionUpdater)});
        }
    }
}
