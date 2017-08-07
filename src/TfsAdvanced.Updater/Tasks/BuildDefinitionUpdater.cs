using System;
using System.Collections.Concurrent;
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

        protected override void Update()
        {
            var buildDefinitions = new ConcurrentBag<BuildDefinition>();
            Parallel.ForEach(projectRepository.GetAll(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, project =>
            {
                var definitions = GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.Builds.BuildDefinition>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/build/definitions?api=2.2").Result;
                if (definitions == null)
                {
                    logger.LogInformation($"Unable to get the definitiosn for the project {project.Name}");
                    return;
                }
                Parallel.ForEach(definitions, new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, definition =>
                {
                    var populatedDefinition = GetAsync.Fetch<TFSAdvanced.Updater.Models.Builds.BuildDefinition>(requestData, definition.url).Result;

                    buildDefinitions.Add(new BuildDefinition
                    {
                        DefaultBranch = populatedDefinition.repository.defaultBranch,
                        Folder = populatedDefinition.path,
                        Id = populatedDefinition.id,
                        Name = populatedDefinition.name,
                        Url = populatedDefinition._links.web.href,
                        Repository = repositoryRepository.GetById(populatedDefinition.repository.id)
                    });
                });
            });

            buildDefinitionRepository.Update(buildDefinitions);

            updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = buildDefinitions.Count, UpdaterName = nameof(BuildDefinitionUpdater)});
        }
    }
}
