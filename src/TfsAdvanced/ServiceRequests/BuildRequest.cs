using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.Infrastructure;

namespace TfsAdvanced.ServiceRequests
{
    public class BuildRequest
    {
        private readonly AppSettings appSettings;
        private readonly Cache cache;
        private string MEMORY_CACHE_KEY = "BUILD_REQUESTS_MEMORY_CACHE_KEY-";

        public BuildRequest(IOptions<AppSettings> appSettings, Cache cache)
        {
            this.cache = cache;
            this.appSettings = appSettings.Value;
        }

        public IList<Build> GetAllBuilds(RequestData requestData)
        {
            IList<Build> cached = cache.Get<IList<Build>>(MEMORY_CACHE_KEY + "all");
            if (cached != null)
                return cached;

            var builds = new List<Build>();
            appSettings.Projects.ForEach(project =>
            {
                builds.AddRange(GetBuilds(requestData, project).Result);
            });

            cache.Put(MEMORY_CACHE_KEY + "all", builds, TimeSpan.FromSeconds(30));

            return builds;
        }

        public async Task<IList<Build>> GetBuilds(RequestData requestData, string tfsProject)
        {
            IList<Build> cached = cache.Get<IList<Build>>(MEMORY_CACHE_KEY + tfsProject);
            if (cached != null)
                return cached;

            var response = await
                requestData.HttpClient.GetStringAsync($"{requestData.BaseAddress}/{tfsProject}/_apis/build/builds?api-version=2.2");
            var responseObject = JsonConvert.DeserializeObject<Response<IEnumerable<Build>>>(response);

            var builds = responseObject.value.ToList();

            builds.ForEach(build => build.buildUrl = $"{requestData.BaseAddress}/{tfsProject}/_build?_a=summary&buildId={build.id}");

            builds = responseObject.value.ToList();

            cache.Put(MEMORY_CACHE_KEY + tfsProject, builds, TimeSpan.FromSeconds(30));

            return builds;
        }
    }
}