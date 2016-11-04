using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.Repository;
using TfsAdvanced.Utilities;

namespace TfsAdvanced.Tasks
{
    public class BuildDefinitionUpdater
    {
        private readonly BuildDefinitionRepository buildDefinitionRepository;
        private readonly BuildRepository buildRepository;
        private readonly ProjectRepository projectRepository;
        private readonly RequestData requestData;
        private bool IsRunning;

        public BuildDefinitionUpdater(BuildDefinitionRepository buildDefinitionRepository, RequestData requestData, ProjectRepository projectRepository, BuildRepository buildRepository)
        {
            this.buildDefinitionRepository = buildDefinitionRepository;
            this.requestData = requestData;
            this.projectRepository = projectRepository;
            this.buildRepository = buildRepository;
        }

        public void Update()
        {
            if (IsRunning)
                return;
            IsRunning = true;
            var buildDefinitions = new ConcurrentBag<BuildDefinition>();
            Parallel.ForEach(projectRepository.GetProjects(), new ParallelOptions {MaxDegreeOfParallelism = Startup.MAX_DEGREE_OF_PARALLELISM}, project =>
            {
                var definitions = GetAsync.FetchResponseList<BuildDefinition>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/build/definitions?api=2.2").Result;
                if (definitions == null)
                    return;
                foreach (var definition in definitions)
                {
                    buildDefinitions.Add(definition);
                }
            });

            buildDefinitionRepository.Update(buildDefinitions.ToList());
            Parallel.ForEach(buildDefinitions, new ParallelOptions {MaxDegreeOfParallelism = Startup.MAX_DEGREE_OF_PARALLELISM}, buildDefinition =>
            {
                var succeededBuilds = buildRepository.GetBuilds(buildDefinition).Where(b => b.status == BuildStatus.completed && b.result == BuildResult.succeeded).ToList();
                buildDefinition.RunTimes = succeededBuilds.Select(b => Convert.ToInt64(((b.finishTime - b.startTime).Value).TotalMilliseconds)).ToList();
                buildDefinition.QueuedTimes = succeededBuilds.Select(b => Convert.ToInt64(((b.startTime - b.queueTime).Value).TotalMilliseconds)).ToList();
                buildDefinition.LatestBuilds = buildRepository.GetLatestBuildOnDefaultBranch(buildDefinition, 5);
            });

            buildDefinitionRepository.Update(buildDefinitions.ToList());
            IsRunning = false;
        }
    }
}
