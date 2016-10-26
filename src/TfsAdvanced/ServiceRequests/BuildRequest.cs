using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.Data.Errors;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Infrastructure;
using TfsAdvanced.Utilities;

namespace TfsAdvanced.ServiceRequests
{
    public class BuildRequest
    {
        private readonly AppSettings appSettings;
        private readonly ProjectServiceRequest projectServiceRequest;
        private readonly Cache cache;
        private string MEMORY_CACHE_KEY = "BUILD_REQUESTS_MEMORY_CACHE_KEY-";

        public BuildRequest(IOptions<AppSettings> appSettings, Cache cache, ProjectServiceRequest projectServiceRequest)
        {
            this.cache = cache;
            this.projectServiceRequest = projectServiceRequest;
            this.appSettings = appSettings.Value;
        }

        public IList<Build> GetAllBuilds(RequestData requestData)
        {
            IList<Build> cached = cache.Get<IList<Build>>(MEMORY_CACHE_KEY + "all");
            if (cached != null)
                return cached;

            var builds = new ConcurrentStack<Build>();
            var projects = projectServiceRequest.GetProjects(requestData);
            Parallel.ForEach(projects, project =>
            {
                builds.PushRange(GetBuilds(requestData, project).Result.ToArray());
            });

            var buildList = builds.ToList();

            cache.Put(MEMORY_CACHE_KEY + "all", buildList, TimeSpan.FromSeconds(30));

            return buildList;
        }

        public async Task<IList<Build>> GetLatestBuildOnDefaultBranch(RequestData requestData, BuildDefinition buildDefinition, int limit)
        {
            var cacheKey = MEMORY_CACHE_KEY + buildDefinition.name + "_limit" + limit;
            IList<Build> cached = cache.Get<IList<Build>>(cacheKey);
            if (cached != null)
                return cached;

            var project = buildDefinition.project;

            var builds = await GetAsync.FetchResponseList<Build>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/build/builds?api-version=2.2&definitions={buildDefinition.id}");

            builds = builds.Where(b => b.sourceBranch == buildDefinition.defaultBranch).OrderByDescending(b => b.id).Take(limit).ToList();

            builds.ForEach(build => build.buildUrl = $"{requestData.BaseAddress}/{project.name}/_build?_a=summary&buildId={build.id}");

            cache.Put(cacheKey, builds, TimeSpan.FromSeconds(30));

            return builds;
        }

        public async Task<IList<Build>> GetBuilds(RequestData requestData, Project project)
        {
            IList<Build> cached = cache.Get<IList<Build>>(MEMORY_CACHE_KEY + project.name);
            if (cached != null)
                return cached;

            var builds = await GetAsync.FetchResponseList<Build>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/build/builds?api-version=2.2");

            builds.ForEach(build => build.buildUrl = $"{requestData.BaseAddress}/{project.name}/_build?_a=summary&buildId={build.id}");

            cache.Put(MEMORY_CACHE_KEY + project.name, builds, TimeSpan.FromSeconds(30));

            return builds;
        }

 
    }
}