using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TfsAdvanced.Models;
using TfsAdvanced.Models.Builds;
using TfsAdvanced.Models.Projects;

namespace TfsAdvanced.ServiceRequests
{
    public class BuildDefinitionRequest
    {
        private readonly RequestData requestData;

        public BuildDefinitionRequest(RequestData requestData)
        {
            this.requestData = requestData;
        }

        public IList<Build> LaunchBuild(IList<BuildDefinition> definitions)
        {
            ConcurrentStack<Build> builds = new ConcurrentStack<Build>();
            Parallel.ForEach(definitions, definition =>
            {
                builds.Push(LaunchBuild(definition));
            });
            return builds.ToList();
        }

        public Build LaunchBuild(BuildDefinition definition)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                                $"{requestData.BaseAddress}/{definition.project.name}/_apis/build/builds?api-version=2.2");
            request.Content =
                new StringContent(
                    JsonConvert.SerializeObject(new BuildQueueRequest
                    {
                        queue = new Id {id = definition.queue.id },
                        definition = new Id { id = definition.id },
                        project = new ProjectGuid { id = definition.project.id },
                        sourceBranch = definition.repository.defaultBranch
                    }), Encoding.UTF8,
                    "application/json");
            var buildResponse = requestData.HttpClient.SendAsync(request).Result;
            var responseString = buildResponse.Content.ReadAsStringAsync().Result;
            if(buildResponse.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Build>(responseString);
            return new Build
            {
                definition = definition,
                project = definition.project,
                buildNumber = "Failed To Start Build"
            };
        }
    }
}