using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TFSAdvanced.Updater.Models.Projects;
using TFSAdvanced.Updater.Tasks;

namespace TfsAdvanced.Updater.Tasks
{
    public class ProjectUpdater : UpdaterBase
    {
        private readonly ProjectRepository projectRepository;
        private readonly RequestData requestData;
        private readonly UpdateStatusRepository updateStatusRepository;

        public ProjectUpdater(ProjectRepository projectRepository, RequestData requestData, UpdateStatusRepository updateStatusRepository, ILogger<ProjectUpdater> logger) : base(logger)
        {
            this.projectRepository = projectRepository;
            this.requestData = requestData;
            this.updateStatusRepository = updateStatusRepository;
        }

        protected override void Update()
        {
            List<Project> projects = GetAsync.FetchResponseList<Project>(requestData, $"{requestData.BaseAddress}/_apis/projects?api-version=1.0", Logger).Result;
            if (projects != null)
            {
                projectRepository.Update(projects.Select(x => new TFSAdvanced.Models.DTO.Project
                {
                    Id = x.id,
                    Name = x.name,
                    Url = x.remoteUrl
                }));
                updateStatusRepository.UpdateStatus(new UpdateStatus { LastUpdate = DateTime.Now, UpdatedRecords = projects.Count, UpdaterName = nameof(ProjectUpdater) });
            }
        }
    }
}