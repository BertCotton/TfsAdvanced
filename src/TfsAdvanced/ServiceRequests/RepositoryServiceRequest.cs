using System;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Repositories;
using TfsAdvanced.Infrastructure;

namespace TfsAdvanced.ServiceRequests
{
    public class RepositoryServiceRequest
    {
        private readonly AppSettings appSettings;
        private IMemoryCache memoryCache;
        private string REPOSITORY_LIST_MEMORY_KEY = "RepositoryListMemoryKey-";

        public RepositoryServiceRequest(IOptions<AppSettings> appSettings, IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
            this.appSettings = appSettings.Value;
        }

        public string BuildDashboardURL(RequestData requestData, Repository repository)
        {
            return $"{requestData.BaseAddress}/{repository.project.name}/_dashboards";
        }

        public IList<Repository> GetAllRepositories(RequestData requestData)
        {
            IList<Repository> cached;
            if (memoryCache.TryGetValue(REPOSITORY_LIST_MEMORY_KEY + "all", out cached))
                return cached;

            var concurrentRepositories = new ConcurrentStack<Repository>();
            Parallel.ForEach(appSettings.Projects, project =>
            {
                var repos = GetRepositories(requestData, project).Result;
                concurrentRepositories.PushRange(repos.ToArray());
            });
            var repositories = concurrentRepositories.ToList();
            memoryCache.Set(REPOSITORY_LIST_MEMORY_KEY + "all", repositories, TimeSpan.FromHours(1));

            return repositories;
        }

        public async Task<IList<Repository>> GetRepositories(RequestData requestData, string tfsProject)
        {
            IList<Repository> cached;
            if (memoryCache.TryGetValue(REPOSITORY_LIST_MEMORY_KEY + tfsProject, out cached))
                return cached;

            var response = await requestData.HttpClient.GetStringAsync($"{requestData.BaseAddress}/{tfsProject}/_apis/git/repositories?api=1.0");
            var responseObject = JsonConvert.DeserializeObject<Response<IEnumerable<Repository>>>(response);
            var repositories = responseObject.value.ToList();
            Parallel.ForEach(repositories, r => r.project.remoteUrl = BuildDashboardURL(requestData, r));

            memoryCache.Set(REPOSITORY_LIST_MEMORY_KEY + tfsProject, repositories, TimeSpan.FromHours(1));

            return repositories;
        }
    }
}