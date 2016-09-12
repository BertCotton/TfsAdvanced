using System;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Infrastructure;

namespace TfsAdvanced.ServiceRequests
{
    public class BuildDefinitionRequest
    {
        private readonly AppSettings appSettings;
        private readonly Cache memoryCache;
        private string MEMORY_CACHE_KEY = "BUILD_DEFINITONS_MEMORY_KEY-";

        public BuildDefinitionRequest(IOptions<AppSettings> appSettings, Cache memoryCache)
        {
            this.memoryCache = memoryCache;
            this.appSettings = appSettings.Value;
        }

        public IList<BuildDefinition> GetAllBuildDefinitions(RequestData requestData)
        {
            IList<BuildDefinition> cached = memoryCache.Get<IList<BuildDefinition>>(MEMORY_CACHE_KEY + "all");
            if(cached != null)
                return cached;
            var buildDefinitions = new List<BuildDefinition>();
            Parallel.ForEach(appSettings.Projects, tfsProject =>
            {
                buildDefinitions.AddRange(GetBuildDefinitions(requestData, tfsProject).Result);
            });
            memoryCache.Set(MEMORY_CACHE_KEY + "all", buildDefinitions, TimeSpan.FromHours(1));
            return buildDefinitions;
        }

        public async Task<IList<BuildDefinition>> GetBuildDefinitions(RequestData requestData, string tfsProject)
        {
            IList<BuildDefinition> cached;
            if (memoryCache.TryGetValue(MEMORY_CACHE_KEY + tfsProject, out cached))
                return cached;
            var response =
                await requestData.HttpClient.GetStringAsync($"{requestData.BaseAddress}/{tfsProject}/_apis/build/definitions?api=2.2");
            var responseObject = JsonConvert.DeserializeObject<Response<IEnumerable<BuildDefinition>>>(response);
            IList<BuildDefinition> buildDefinitions =  responseObject.value.ToList();

            memoryCache.Set(MEMORY_CACHE_KEY + "all", buildDefinitions, TimeSpan.FromHours(1));

            return buildDefinitions;
        }

        public void LaunchBuild(RequestData requestData, IList<BuildDefinition> definitions)
        {
            Parallel.ForEach(definitions, definition =>
            {
                LaunchBuild(requestData, definition);
            });
        }

        public void LaunchBuild(RequestData requestData, BuildDefinition definition)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                                $"{requestData.BaseAddress}/{definition.project.name}/_apis/build/builds?api-version=2.2");
            request.Content =
                new StringContent(
                    JsonConvert.SerializeObject(new BuildQueueRequest
                    {
                        definition = new Id { id = definition.id },
                        project = new ProjectGuid { id = definition.project.id }
                    }), Encoding.UTF8,
                    "application/json");
            var buildResponse = requestData.HttpClient.SendAsync(request).Result;
            var responseString = buildResponse.Content.ReadAsStringAsync().Result;
            Debug.WriteLine(buildResponse.StatusCode);
        }
    }
}