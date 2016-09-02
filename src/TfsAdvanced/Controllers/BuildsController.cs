using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.ServiceRequests;

namespace TfsAdvanced.Controllers
{
    [Route("data/Builds")]
    public class BuildsController : Controller
    {
        private static string BUILD_MEMORY_KEY = "Builds";
        private readonly BuildRequest buildRequest;
        private readonly IMemoryCache memoryCache;
        private readonly RequestData requestData;

        public BuildsController(IMemoryCache memoryCache, RequestData requestData, BuildRequest buildRequest)
        {
            this.memoryCache = memoryCache;
            this.requestData = requestData;
            this.buildRequest = buildRequest;
        }

        [HttpGet]
        public IList<Build> Index()
        {
            List<Build> cachedBuilds;
            if (memoryCache.TryGetValue(BUILD_MEMORY_KEY, out cachedBuilds))
                return cachedBuilds;

            var builds = buildRequest.GetAllBuilds(requestData);

            memoryCache.Set(BUILD_MEMORY_KEY, builds,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(10)));

            return builds;
        }
    }
}