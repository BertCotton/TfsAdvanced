using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly ProjectRepository projectRepository;
        private readonly RepositoryRepository repositoryRepository;
        private readonly RequestData requestData;
        private readonly UpdateStatusRepository updateStatusRepository;

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
            Parallel.ForEach(projectRepository.GetAll(), new ParallelOptions { MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM }, project =>
              {
                  List<TFSAdvanced.Updater.Models.Builds.BuildDefinition> definitions = GetAsync.FetchResponseList<TFSAdvanced.Updater.Models.Builds.BuildDefinition>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/build/definitions?api=2.2", Logger).Result;
                  if (definitions == null)
                  {
                      Logger.LogInformation($"Unable to get the definition for the project {project.Name}");
                      return;
                  }
                  Parallel.ForEach(definitions, new ParallelOptions { MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM }, definition =>
                  {
                      TFSAdvanced.Updater.Models.Builds.BuildDefinition populatedDefinition = GetAsync.Fetch<TFSAdvanced.Updater.Models.Builds.BuildDefinition>(requestData, definition.url).Result;
                      Repository repository = repositoryRepository.GetById(populatedDefinition.repository.id);
                      if (repository == null)
                          Logger.LogDebug($"Repository no found for build definition {populatedDefinition.name} and repository {populatedDefinition.repository.name}");

                      buildDefinitions.Add(new BuildDefinition
                      {
                          DefaultBranch = populatedDefinition.repository.defaultBranch,
                          Folder = populatedDefinition.path,
                          Id = populatedDefinition.id,
                          Name = populatedDefinition.name,
                          Url = populatedDefinition._links.web.href,
                          Repository = repository
                      });
                  });
              });

            buildDefinitionRepository.Update(buildDefinitions);

            updateStatusRepository.UpdateStatus(new UpdateStatus { LastUpdate = DateTime.Now, UpdatedRecords = buildDefinitions.Count, UpdaterName = nameof(BuildDefinitionUpdater) });
        }
    }
}