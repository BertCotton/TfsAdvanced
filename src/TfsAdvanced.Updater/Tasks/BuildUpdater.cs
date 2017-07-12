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
    public class BuildUpdater
    {
        private readonly BuildRepository buildRepository;
        private readonly UpdateStatusRepository updateStatusRepository;
        private readonly ProjectRepository projectRepository;
        private readonly RequestData requestData;
        private bool IsRunning;

        public BuildUpdater(BuildRepository buildRepository, RequestData requestData, ProjectRepository projectRepository, UpdateStatusRepository updateStatusRepository)
        {
            this.buildRepository = buildRepository;
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
                
                var builds = new ConcurrentStack<Build>();
                Parallel.ForEach(projectRepository.GetAll(), new ParallelOptions {MaxDegreeOfParallelism = AppSettings.MAX_DEGREE_OF_PARALLELISM}, project =>
                {
                    var projectBuilds = GetAsync.FetchResponseList<Build>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/build/builds?api-version=2.2").Result;
                    if (projectBuilds != null && projectBuilds.Any())
                    {
                        builds.PushRange(projectBuilds.ToArray());
                    }
                });

                // The builds must be requested without the filter because the only filter available is minFinishTime, which will filter out those that haven't finished yet
                DateTime yesterday = DateTime.Now.Date.AddDays(-1);
                var buildLists = builds.Where(x => x.startTime >= yesterday).ToList();
                buildRepository.Update(buildLists);
                updateStatusRepository.UpdateStatus(new UpdateStatus {LastUpdate = DateTime.Now, UpdatedRecords = buildLists.Count, UpdaterName = nameof(BuildUpdater)});


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
