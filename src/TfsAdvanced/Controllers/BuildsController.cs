using TfsAdvanced.Data;
using TfsAdvanced.ServiceRequests;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace TfsAdvanced.Controllers
{
    [Route("data/Builds")]
    public class BuildsController : Controller
    {
        private static string BUILD_MEMORY_KEY = "Builds";
        private readonly BuildRequest buildRequest;
        private readonly IMemoryCache memoryCache;
        private readonly TfsRequest tfsRequest;

        public BuildsController(IMemoryCache memoryCache, TfsRequest tfsRequest, BuildRequest buildRequest)
        {
            this.memoryCache = memoryCache;
            this.tfsRequest = tfsRequest;
            this.buildRequest = buildRequest;
        }

        [HttpGet]
        public IList<Build> Index()
        {
            List<Build> cachedBuilds;
            if (memoryCache.TryGetValue(BUILD_MEMORY_KEY, out cachedBuilds))
                return cachedBuilds;

            using (var requestData = tfsRequest.GetHttpClient())
            {
                var builds = buildRequest.GetAllBuilds(requestData);

                memoryCache.Set(BUILD_MEMORY_KEY, builds,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(10)));

                return builds;
            }
        }
    }
}