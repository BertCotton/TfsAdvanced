using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Data.Repositories;
using TfsAdvanced.Infrastructure;
using TfsAdvanced.Utilities;

namespace TfsAdvanced.ServiceRequests
{
    public class ProjectServiceRequest
    {
        private static string PROJECT_MEM_KEY = "Projects";
        private readonly AppSettings appSettings;
        private readonly Cache cache;

        public ProjectServiceRequest(IOptions<AppSettings> appSettings, Cache cache)
        {
            this.cache = cache;
            this.appSettings = appSettings.Value;
        }

        public Project GetProject(RequestData requestData, Repository repo)
        {
            string cacheKey = PROJECT_MEM_KEY + "-" + repo.id;
            Project cached = cache.Get<Project>(cacheKey);
            if (cached != null)
                return cached;
            var projectsResponse = requestData.HttpClient.GetStringAsync(repo.url).Result;
            Project project = JsonConvert.DeserializeObject<Project>(projectsResponse);

            cache.Put(cacheKey, project, TimeSpan.FromHours(1));

            return project;
        }

        public List<Project> GetProjects(RequestData requestData)
        {
            List<Project> cachedProjects = cache.Get<List<Project>>(PROJECT_MEM_KEY);
            if (cachedProjects != null)
                return cachedProjects;

            List<Project> projects = new List<Project>();

            Parallel.ForEach(appSettings.Projects, projectName =>
            {
                var project = GetAsync.Fetch<Project>(requestData, $"{requestData.BaseAddress}/_apis/projects/{projectName}?api-version=1.0").Result;
                projects.Add(project);
            });
            
            cache.Put(PROJECT_MEM_KEY, projects, TimeSpan.FromHours(1));

            return projects;
        }

        public Dictionary<string, KeyValuePair<Repository, Project>> GetProjects(RequestData requestData,
            IList<Repository> repos)
        {
            var projects = new ConcurrentDictionary<string, KeyValuePair<Repository, Project>>();
            Parallel.ForEach(repos, repository =>
            {
                projects.TryAdd(repository.id,
                    new KeyValuePair<Repository, Project>(repository, GetProject(requestData, repository)));
            });

            return projects.ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}