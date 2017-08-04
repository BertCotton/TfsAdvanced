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
using BuildDefinition = TFSAdvanced.Updater.Models.Builds.BuildDefinition;

namespace TfsAdvanced.Updater.Tasks
{
    public class BuildDefinitionUpdater
    {
        private readonly BuildDefinitionRepository buildDefinitionRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly ProjectRepository projectRepository;
        private readonly RepositoryRepository repositoryRepository;
        private readonly RequestData requestData;
        private bool IsRunning;

        public BuildDefinitionUpdater(BuildDefinitionRepository buildDefinitionRepository, RequestData requestData, ProjectRepository projectRepository, UpdateStatusRepository updateStatusRepository, RepositoryRepository repositoryRepository)
        {
            this.buildDefinitionRepository = buildDefinitionRepository;
            this.requestData = requestData;
            this.projectRepository = projectRepository;
            this.updateStatusRepository = updateStatusRepository;
            this.repositoryRepository = repositoryRepository;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Update()
        {
            if (IsRunning)
                return;
            IsRunning = true;
            try
            {

                var buildDefinitions = new ConcurrentBag<TFSAdvanced.Models.DTO.BuildDefinition>();
                Parallel.ForEach(projectRepository.GetAll(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, project =>
                {
                    var definitions = GetAsync.FetchResponseList<BuildDefinition>(requestData, $"{requestData.BaseAddress}/{project.Name}/_apis/build/definitions?api=2.2").Result;
                    if (definitions == null)
                        return;
                    Parallel.ForEach(definitions, new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, definition =>
                    {
                        var populatedDefinition = GetAsync.Fetch<BuildDefinition>(requestData, definition.url).Result;

                        buildDefinitions.Add(new TFSAdvanced.Models.DTO.BuildDefinition
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
                
                updateStatusRepository.UpdateStatus(new UpdateStatus { LastUpdate = DateTime.Now, UpdatedRecords = buildDefinitions.Count, UpdaterName = nameof(BuildDefinitionUpdater)});

    }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error processing the build definition updater.", ex);
            }
            finally

            {
                IsRunning = false;
            }


        }
    }
}
