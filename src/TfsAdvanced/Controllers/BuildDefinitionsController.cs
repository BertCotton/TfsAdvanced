using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.ServiceRequests;

namespace TfsAdvanced.Controllers
{
    [Route("data/BuildDefinitions")]
    public class BuildDefinitionsController : Controller
    {
        private readonly BuildDefinitionRequest buildDefinitionRequest;
        private readonly RequestData requestData;

        public BuildDefinitionsController(BuildDefinitionRequest buildDefinitionRequest, RequestData requestData)
        {
            this.buildDefinitionRequest = buildDefinitionRequest;
            this.requestData = requestData;
        }

        [HttpPost]
        public async Task<IActionResult> BuildDefinitions([FromBody] List<int> definitionIds)
        {
            if (!definitionIds.Any())
                return NotFound();

            var definitions =
                (await buildDefinitionRequest.GetAllBuildDefinitions(requestData))
                    .Where(x => definitionIds.Contains(x.id))
                    .ToList();
            buildDefinitionRequest.LaunchBuild(requestData, definitions);

            return Ok();
        }

        [HttpGet("UpdateCI")]
        public async Task<IActionResult> SetCIBuild([FromQuery] bool JustUpdate)
        {
            IList<BuildDefinition> updated = new List<BuildDefinition>();
            IList<BuildDefinition> builds = await buildDefinitionRequest.GetAllBuildDefinitions(requestData);
            foreach (var buildDefinition in builds)
            {
                if(buildDefinition.project.name != "Benefits")
                    continue;

                
                var response =
                    await requestData.HttpClient.GetStringAsync($"{buildDefinition.url}");
                var responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                if (!responseObject["path"].ToString().Contains("Release"))
                    continue;
                
                var build = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        {"enabled", true},
                        {"continueOnError", true },
                        {"alwaysRun", true },
                        {"displayName", "Task group: Package Release Publish" },
                        {"timeoutInMinutes", 0 },
                        {"task", new Dictionary<string, object>
                            {
                                {"id", "be21847e-32f6-4b40-9325-1d475a763e47" },
                                {"versionSpec", "*" },
                                {"definitionType", "metaTask" }
                            }
                        },
                        {"inputs", new Dictionary<string, object>() }
                    }
                };

                responseObject["build"] = build;
                responseObject["buildNumberFormat"] = "$(date:yyyyMMdd)$(rev:.r)";

                var method = HttpMethod.Put;
                if (!JustUpdate)
                {
                    var deleted = await DeleteBuildDefinition(buildDefinition);

                    if (!deleted)
                        continue;
                    method = HttpMethod.Post;
                }
                var request = new HttpRequestMessage(method, $"{buildDefinition.url}?api-version=2.0");
                request.Content = new StringContent(JsonConvert.SerializeObject(responseObject), Encoding.UTF8, "application/json");
                var saveResponse = await requestData.HttpClient.SendAsync(request);
                var statusCode = saveResponse.StatusCode;
                var responseMessage = await saveResponse.Content.ReadAsStringAsync();
                Debug.WriteLine($"Updated {buildDefinition.name}: {responseMessage}");
                updated.Add(buildDefinition);

            }
            return Ok(updated);
        }

        private async Task<Boolean> DeleteBuildDefinition(BuildDefinition buildDefinition)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{buildDefinition.url}?api-version=2.0");
            var saveResponse = await requestData.HttpClient.SendAsync(request);
            var statusCode = saveResponse.StatusCode;
            var responseMessage = await saveResponse.Content.ReadAsStringAsync();
            return (statusCode == HttpStatusCode.NoContent);

        }

        [HttpGet]
        public async Task<IList<BuildDefinition>> Index()
        {
            return await buildDefinitionRequest.GetAllBuildDefinitions(requestData);
        }
    }
}