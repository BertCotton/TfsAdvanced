using System;
using System.Linq;
using Hangfire;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TFSAdvanced.Updater.Models.Projects;

namespace TfsAdvanced.Updater.Tasks
{
    public class ProjectUpdater
    {

        private readonly ProjectRepository projectRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly RequestData requestData;
        private bool IsRunning;

        public ProjectUpdater(ProjectRepository projectRepository, RequestData requestData, UpdateStatusRepository updateStatusRepository)
        {
            this.projectRepository = projectRepository;
            this.requestData = requestData;
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
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error running project updater.", ex);
            }
            finally
            {
                IsRunning = false;
            }

        }
    }
}
