using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Infrastructure;

namespace TfsAdvanced.ServiceRequests
{
    public class BuildDefinitionRequest
    {
        private readonly AppSettings appSettings;

        public BuildDefinitionRequest(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public IList<BuildDefinition> GetAllBuildDefinitions(RequestData requestData)
        {
            var buildDefinitions = new List<BuildDefinition>();
            Parallel.ForEach(appSettings.Projects, tfsProject =>
            {
                buildDefinitions.AddRange(GetBuildDefinitions(requestData, tfsProject).Result);
            });
            return buildDefinitions;
        }

        public async Task<IList<BuildDefinition>> GetBuildDefinitions(RequestData requestData, string tfsProject)
        {
            var response =
                await requestData.HttpClient.GetStringAsync($"{requestData.BaseAddress}/{tfsProject}/_apis/build/definitions?api=2.2");
            var responseObject = JsonConvert.DeserializeObject<Response<IEnumerable<BuildDefinition>>>(response);
            return responseObject.value.ToList();
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
        }
    }
}