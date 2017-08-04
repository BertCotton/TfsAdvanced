using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TfsAdvanced.Models;
using TFSAdvanced.Updater.Models.Builds;
using TFSAdvanced.Updater.Models.Projects;
using Build = TFSAdvanced.Models.DTO.Build;
using BuildDefinition = TFSAdvanced.Models.DTO.BuildDefinition;

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
                                $"{requestData.BaseAddress}/{definition.Repository.Project.Name}/_apis/build/builds?api-version=2.2");
            request.Content =
                new StringContent(
                    JsonConvert.SerializeObject(new BuildQueueRequest
                    {
                        queue = new Id {id = definition.QueueId},
                        definition = new Id { id = definition.Id },
                        project = new ProjectGuid { id = definition.Repository.Project.Id },
                        sourceBranch = definition.DefaultBranch
                    }), Encoding.UTF8,
                    "application/json");
            var buildResponse = requestData.HttpClient.SendAsync(request).Result;
            var responseString = buildResponse.Content.ReadAsStringAsync().Result;
            if(buildResponse.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Build>(responseString);
            return null;
        }
    }
}