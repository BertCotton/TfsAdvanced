using System;
using System.Linq;
using Hangfire;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Projects;

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
                    projectRepository.Update(projects.ToList());
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
