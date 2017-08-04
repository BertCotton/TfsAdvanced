using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.ServiceRequests;
using TfsAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.Web.Controllers
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

        [HttpPost("{definitionId}")]
        public IActionResult BuildDefinitions([FromRoute] int definitionId)
        {
            var definition = buildDefinitionRepository.GetBuildDefinition(definitionId);
            if (definition == null)
                return NotFound(definitionId);
            var build = buildDefinitionRequest.LaunchBuild(definition);

            return Ok(build);
        }

        [HttpPost]
        public IActionResult BuildDefinitions([FromBody] List<int> definitionIds)
        {
            if (!definitionIds.Any())
                return NotFound();

            var definitions = buildDefinitionRepository.GetAll()
                    .Where(x => definitionIds.Contains(x.Id))
                    .ToList();
            var builds = buildDefinitionRequest.LaunchBuild(definitions);

            return Ok(builds);
        }
        
        [HttpGet]
        public IEnumerable<BuildDefinition> Index()
        {
            return buildDefinitionRepository.GetAll();
        }
    }
}