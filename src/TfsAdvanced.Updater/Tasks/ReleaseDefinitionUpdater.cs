using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Infrastructure;
using TfsAdvanced.Updater;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TFSAdvanced.Updater.Tasks
{
    public class ReleaseDefinitionUpdater : UpdaterBase
    {
        private readonly ReleaseDefinitionRepository releaseDefinitionRepository;
        private readonly ProjectRepository projectRepository;
        private readonly RequestData requestData;
     
        public ReleaseDefinitionUpdater(ReleaseDefinitionRepository releaseDefinitionRepository, ProjectRepository projectRepository, RequestData requestData, ILogger<ReleaseDefinitionUpdater> logger) : base(logger)
        {
            this.releaseDefinitionRepository = releaseDefinitionRepository;
            this.projectRepository = projectRepository;
            this.requestData = requestData;
        }

        protected override void Update()
        {
            var releases = new ConcurrentStack<ReleaseDefinition>();
            Parallel.ForEach(projectRepository.GetAll(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, project =>
            {

                var releaseDefinitions = GetAsync.FetchResponseList<Models.Releases.ReleaseDefinition>(requestData, $"{requestData.BaseReleaseManagerAddress}/{project.Id}/_apis/Release/definitions?api-version=3.0-preview.1").Result;
                if (releaseDefinitions != null)
                {
                    Parallel.ForEach(releaseDefinitions, new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, releaseDefinition =>
                    {
                        var populatedReleaseDefinition = GetAsync.Fetch<Models.Releases.ReleaseDefinition>(requestData, releaseDefinition.url).Result;
                        releases.Push(CreateReleaseDefinition(populatedReleaseDefinition));
                    });
                }
            });

            releaseDefinitionRepository.Update(releases);
        }

        private ReleaseDefinition CreateReleaseDefinition(Models.Releases.ReleaseDefinition releaseDefinition)
        {
            var dto = new ReleaseDefinition
            {
                Name = releaseDefinition.name,
                Id = releaseDefinition.id
            };

            if (releaseDefinition.artifacts != null)
            {
                var artifact = releaseDefinition.artifacts.FirstOrDefault(x => x.isPrimary);
                if (artifact != null && artifact.definitionReference != null && artifact.definitionReference.project != null)
                {
                    var projectId = artifact.definitionReference.project;
                    dto.Project = projectRepository.GetProject(projectId.id);
                }
            }

            return dto;

        }
    }
}
