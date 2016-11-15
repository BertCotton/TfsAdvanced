using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.Repository;
using TfsAdvanced.Utilities;

namespace TfsAdvanced.Tasks
{
    public class BuildUpdater
    {
        private readonly BuildRepository buildRepository;
        private readonly ProjectRepository projectRepository;
        private readonly RequestData requestData;
        private bool IsRunning;

        public BuildUpdater(BuildRepository buildRepository, RequestData requestData, ProjectRepository projectRepository)
        {
            this.buildRepository = buildRepository;
            this.requestData = requestData;
            this.projectRepository = projectRepository;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Update()
        {
            if (IsRunning)
                return;
            IsRunning = true;
            try
            {

                var builds = new ConcurrentStack<Build>();
                Parallel.ForEach(projectRepository.GetProjects(), new ParallelOptions {MaxDegreeOfParallelism = Startup.MAX_DEGREE_OF_PARALLELISM}, project =>
                {
                    var projectBuilds = GetAsync.FetchResponseList<Build>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/build/builds?api-version=2.2").Result;
                    if (projectBuilds != null && projectBuilds.Any())
                    {
                        builds.PushRange(projectBuilds.ToArray());
                    }
                });

                buildRepository.Update(builds.ToList());


            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error running build updater", ex);
            }
            finally
            {
                IsRunning = false;
            }
        }
    }
}
