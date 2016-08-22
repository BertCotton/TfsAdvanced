using TfsAdvanced.Data;
using TfsAdvanced.ServiceRequests;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TfsAdvanced.Controllers
{
    [Route("data/BuildDefinitions")]
    public class BuildDefinitionsController : Controller
    {
        private static string DEFINITONS_CACHE_KEY = "BuildDefinitions";
        private readonly BuildDefinitionRequest buildDefinitionRequest;
        private readonly IMemoryCache memoryCache;
        private readonly TfsRequest tfsRequest;

        public BuildDefinitionsController(TfsRequest tfsRequest, BuildDefinitionRequest buildDefinitionRequest, IMemoryCache memoryCache)
        {
            this.tfsRequest = tfsRequest;
            this.buildDefinitionRequest = buildDefinitionRequest;
            this.memoryCache = memoryCache;
        }

        [HttpPost]
        public IActionResult BuildDefinitions(List<int> definitionIds)
        {
            if (!definitionIds.Any())
                return HttpNotFound();

            using (var requestData = tfsRequest.GetHttpClient())
            {
                var definitions = buildDefinitionRequest.GetAllBuildDefinitions(requestData).Where(x => definitionIds.Contains(x.id)).ToList();
                buildDefinitionRequest.LaunchBuild(requestData, definitions);
            }
            return Ok();
        }

        [HttpGet]
        public IList<BuildDefinition> Index()
        {
            List<BuildDefinition> cacheDefinitions;
            if (memoryCache.TryGetValue(DEFINITONS_CACHE_KEY, out cacheDefinitions))
            {
                return cacheDefinitions;
            }

            using (var requestData = tfsRequest.GetHttpClient())
            {
                var definitions = buildDefinitionRequest.GetAllBuildDefinitions(requestData);
                memoryCache.Set(DEFINITONS_CACHE_KEY, definitions,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1)));

                return definitions;
            }
        }
    }
}