using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.Infrastructure;

namespace TfsAdvanced.ServiceRequests
{
    public class BuildRequest
    {
        private readonly AppSettings appSettings;
        private readonly IMemoryCache memoryCache;
        private string MEMORY_CACHE_KEY = "BUILD_REQUESTS_MEMORY_CACHE_KEY-";

        public BuildRequest(IOptions<AppSettings> appSettings, IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
            this.appSettings = appSettings.Value;
        }

        public IList<Build> GetAllBuilds(RequestData requestData)
        {
            IList<Build> cached;
            if(memoryCache.TryGetValue(MEMORY_CACHE_KEY + "all", out cached))
                return cached;
            
            var builds = new List<Build>();
            appSettings.Projects.ForEach(project =>
            {
                builds.AddRange(GetBuilds(requestData, project).Result);
            });

            memoryCache.Set(MEMORY_CACHE_KEY + "all", builds, TimeSpan.FromSeconds(30));

            return builds;
        }

        public async Task<IList<Build>> GetBuilds(RequestData requestData, string tfsProject)
        {
            IList<Build> cached;
            if (memoryCache.TryGetValue(MEMORY_CACHE_KEY + tfsProject, out cached))
                return cached;

            var response = await
                requestData.HttpClient.GetStringAsync($"{requestData.BaseAddress}/{tfsProject}/_apis/build/builds?api-version=2.2&minFinishTime={DateTime.Now.Date.AddDays(-1).ToString("o")}");
            var responseObject = JsonConvert.DeserializeObject<Response<IEnumerable<Build>>>(response);

            var builds = responseObject.value.ToList();

            builds.ForEach(build => build.buildUrl = $"{requestData.BaseAddress}/{tfsProject}/_build?_a=summary&buildId={build.id}");

            builds = responseObject.value.ToList();

            memoryCache.Set(MEMORY_CACHE_KEY + tfsProject, builds, TimeSpan.FromSeconds(30));

            return builds;
        }
    }
}