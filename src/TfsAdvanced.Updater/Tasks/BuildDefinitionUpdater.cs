using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Builds;
using TfsAdvanced.Models.Infrastructure;

namespace TfsAdvanced.Updater.Tasks
{
    public class BuildDefinitionUpdater
    {
        private readonly BuildDefinitionRepository buildDefinitionRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly ProjectRepository projectRepository;
        private readonly RequestData requestData;
        private bool IsRunning;

        public BuildDefinitionUpdater(BuildDefinitionRepository buildDefinitionRepository, RequestData requestData, ProjectRepository projectRepository, UpdateStatusRepository updateStatusRepository)
        {
            this.buildDefinitionRepository = buildDefinitionRepository;
            this.requestData = requestData;
            this.projectRepository = projectRepository;
            this.updateStatusRepository = updateStatusRepository;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Update()
        {
            if (IsRunning)
                return;
            IsRunning = true;
            try
            {

                var buildDefinitions = new ConcurrentBag<BuildDefinition>();
                Parallel.ForEach(projectRepository.GetAll(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, project =>
                {
                    var definitions = GetAsync.FetchResponseList<BuildDefinition>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/build/definitions?api=2.2").Result;
                    if (definitions == null)
                        return;
                    Parallel.ForEach(definitions, new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, definition =>
                    {
                        var populatedDefinition = GetAsync.Fetch<BuildDefinition>(requestData, definition.url).Result;
                        buildDefinitions.Add(populatedDefinition);
                    });
                });

                buildDefinitionRepository.Update(buildDefinitions.ToList());
                
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
