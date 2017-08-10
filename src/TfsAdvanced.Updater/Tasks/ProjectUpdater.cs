using System;
using System.Linq;
using Hangfire;
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
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly RequestData requestData;

        public ProjectUpdater(ProjectRepository projectRepository, RequestData requestData, UpdateStatusRepository updateStatusRepository, ILogger<ProjectUpdater> logger) : base(logger)
        {
            this.projectRepository = projectRepository;
            this.requestData = requestData;
            this.updateStatusRepository = updateStatusRepository;
        }

        protected override void Update()
        {
            var projects = GetAsync.FetchResponseList<Project>(requestData, $"{requestData.BaseAddress}/_apis/projects?api-version=1.0").Result;
            if (projects != null)
            {
                projectRepository.Update(projects.Select(x => new TFSAdvanced.Models.DTO.Project
                {
                    Id = x.id,
                    Name = x.name,
                    Url = x.remoteUrl
                }));
                updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = projects.Count, UpdaterName = nameof(ProjectUpdater)});
            }
        }
    }
}
