using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Infrastructure;
using TfsAdvanced.Utilities;

namespace TfsAdvanced.ServiceRequests
{
    public class BuildDefinitionRequest
    {
        private readonly AppSettings appSettings;
        private readonly ProjectServiceRequest projectServiceRequest;
        private readonly Cache cache;
        private string MEMORY_CACHE_KEY = "BUILD_DEFINITONS_MEMORY_KEY-";

        public BuildDefinitionRequest(IOptions<AppSettings> appSettings, Cache cache, ProjectServiceRequest projectServiceRequest)
        {
            this.cache = cache;
            this.projectServiceRequest = projectServiceRequest;
            this.appSettings = appSettings.Value;
        }

        public async Task<IList<BuildDefinition>> GetAllBuildDefinitions(RequestData requestData)
        {
            IList<BuildDefinition> cached = cache.Get<IList<BuildDefinition>>(MEMORY_CACHE_KEY + "all");
            if (cached != null)
                return cached;
            var buildDefinitions = new List<BuildDefinition>();
            var projects = await projectServiceRequest.GetProjects(requestData);
            Parallel.ForEach(projects, project =>
            {
                buildDefinitions.AddRange(GetBuildDefinitions(requestData, project).Result);
            });
            cache.Put(MEMORY_CACHE_KEY + "all", buildDefinitions, TimeSpan.FromHours(1));
            return buildDefinitions;
        }

        public async Task<IList<BuildDefinition>> GetBuildDefinitions(RequestData requestData, Project project)
        {
            IList<BuildDefinition> cached = cache.Get<IList<BuildDefinition>>(MEMORY_CACHE_KEY + project.name);
            if (cached != null)
                return cached;
            var buildDefinitions = await GetAsync.FetchResponseList<BuildDefinition>(requestData, $"{requestData.BaseAddress}/{project.name}/_apis/build/definitions?api=2.2");

            cache.Put(MEMORY_CACHE_KEY + "all", buildDefinitions, TimeSpan.FromHours(1));

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