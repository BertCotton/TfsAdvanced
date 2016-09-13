using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.Infrastructure;

namespace TfsAdvanced.Controllers
{
    [Route("CacheReport")]
    public class CacheReportController : Controller
    {
        private readonly CacheStats cacheStats;

        public CacheReportController(CacheStats cacheStats)
        {
            this.cacheStats = cacheStats;
        }

        [HttpGet]
        public IActionResult GetCacheStats()
        {
            Dictionary<string, object> response = new Dictionary<string, object>();

            response["hitCount"] = cacheStats.HitCount();
            response["missCount"] = cacheStats.MissCount();
            response["hitRate"] = cacheStats.HitRate();
            response["evictionCount"] = cacheStats.EvictionCount();

            return Ok(response);
        }
    }
}

