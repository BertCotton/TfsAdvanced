using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly ProjectRepository projectRepository;
        private readonly BuildRepository buildRepository;
        private readonly RequestData requestData;
     
        public ReleaseDefinitionUpdater(ReleaseDefinitionRepository releaseDefinitionRepository, ProjectRepository projectRepository, RequestData requestData, ILogger<ReleaseDefinitionUpdater> logger, UpdateStatusRepository updateStatusRepository, BuildRepository buildRepository) : base(logger)
        {
            this.releaseDefinitionRepository = releaseDefinitionRepository;
            this.projectRepository = projectRepository;
            this.requestData = requestData;
            this.updateStatusRepository = updateStatusRepository;
            this.buildRepository = buildRepository;
        }

        protected override async Task Update(bool initialize)
        {
            if (initialize && !releaseDefinitionRepository.IsEmpty())
                return;


            IList<ReleaseDefinition> releaseDefinitions = new List<ReleaseDefinition>();
            foreach (var project in projectRepository.GetAll())
            {

                foreach (var releaseDefinition in await GetAsync.FetchResponseList<Models.Releases.ReleaseDefinition>(requestData, $"{requestData.BaseReleaseManagerAddress}/{project.ProjectId}/_apis/Release/definitions?api-version=3.0-preview.1"))
                {
                    var populatedReleaseDefinition = await GetAsync.Fetch<Models.Releases.ReleaseDefinition>(requestData, releaseDefinition.url);

                    if (populatedReleaseDefinition.artifacts == null)
                    {
                        logger.LogWarning($"Unable to locate artifact for release definition #{populatedReleaseDefinition.id}");
                        continue;
                    }

                    var dto = new ReleaseDefinition
                    {
                        Name = populatedReleaseDefinition.name,
                        ReleaseDefinitionId = populatedReleaseDefinition.id,
                        Project = project,
                        ProjectId = project.Id
                    };

                    var artifact = populatedReleaseDefinition.artifacts.FirstOrDefault(x => x.isPrimary);
                    if (artifact == null)
                    {
                        logger.LogWarning($"Unable to find the primary release definition for release definition {populatedReleaseDefinition.id}");
                    }
                    else if (artifact.definitionReference?.project != null && artifact.definitionReference?.definition != null)
                    {
                        var projectId = artifact.definitionReference.project.id;
                        var definitionId = Convert.ToInt32(artifact.definitionReference.definition.id);
                        var build = buildRepository.GetByDefinitionAndProject(definitionId, projectId);
                        if (build != null)
                        {
                            dto.SourceRepository = build.Repository;
                            dto.Build = build;
                        }
                        else
                        {
                            logger.LogWarning($"Unable to find build for project {projectId} and definition {definitionId}");
                        }
                    }

                    releaseDefinitions.Add(dto);

                }
            }

           await releaseDefinitionRepository.Update(releaseDefinitions);
            updateStatusRepository.UpdateStatus(new UpdateStatus { LastUpdate = DateTime.Now, UpdatedRecords = releaseDefinitions.Count, UpdaterName = GetType().Name });

        }
    }
}
