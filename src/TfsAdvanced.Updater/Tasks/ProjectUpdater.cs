using System;
using System.Linq;
using System.Threading.Tasks;
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

        protected override async Task Update(bool initialize)
        {
            if (initialize && !projectRepository.IsEmpty())
                return;


            var projects = (await GetAsync.FetchResponseList<Project>(requestData, $"{requestData.BaseAddress}/_apis/projects?api-version=1.0")).ToList();
            await projectRepository.Update(projects.Select(x => new TFSAdvanced.Models.DTO.Project
            {
                ProjectId = x.id,
                Name = x.name,
                Url = x.url
            }).ToList());
            updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = projects.Count(), UpdaterName = nameof(ProjectUpdater)});
        }
    }
}
