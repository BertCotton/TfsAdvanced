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
using TfsAdvanced.Repository;
using TfsAdvanced.ServiceRequests;

namespace TfsAdvanced.Controllers
{
    [Route("data/BuildDefinitions")]
    public class BuildDefinitionsController : Controller
    {
        private readonly BuildDefinitionRequest buildDefinitionRequest;
        private readonly BuildDefinitionRepository buildDefinitionRepository;
        

        public BuildDefinitionsController(BuildDefinitionRequest buildDefinitionRequest, BuildDefinitionRepository buildDefinitionRepository)
        {
            this.buildDefinitionRequest = buildDefinitionRequest;
            this.buildDefinitionRepository = buildDefinitionRepository;
        }

        [HttpPost]
        public IActionResult BuildDefinitions([FromBody] List<int> definitionIds)
        {
            if (!definitionIds.Any())
                return NotFound();

            var definitions = buildDefinitionRepository.GetBuildDefinitions()
                    .Where(x => definitionIds.Contains(x.id))
                    .ToList();
            var builds = buildDefinitionRequest.LaunchBuild(definitions);

            return Ok(builds);
        }
        
        [HttpGet]
        public IList<BuildDefinition> Index()
        {
            return buildDefinitionRepository.GetBuildDefinitions();
        }
    }
}