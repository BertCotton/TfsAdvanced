using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Data.PullRequests;
using TfsAdvanced.Repository;
using TfsAdvanced.Utilities;

namespace TfsAdvanced.Tasks
{
    public class ProjectUpdater
    {
        
        private readonly ProjectRepository projectRepository;
        private readonly RequestData requestData;
        private bool IsRunning;

        public ProjectUpdater(ProjectRepository projectRepository, RequestData requestData)
        {
            this.projectRepository = projectRepository;
            this.requestData = requestData;
        }

        public void Update()
        {
            if (IsRunning)
                return;
            IsRunning = true;
            var projects = GetAsync.FetchResponseList<Project>(requestData, $"{requestData.BaseAddress}/_apis/projects?api-version=1.0").Result;
            if(projects != null)
                projectRepository.Update(projects.ToList());

            IsRunning = false;

        }
    }
}
