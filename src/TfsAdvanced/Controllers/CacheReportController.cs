using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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

            var hitCount = cacheStats.HitCount();
            var missCount = cacheStats.MissCount();
            var evictionCount = cacheStats.EvictionCount();
            var upTime = (DateTime.Now - cacheStats.StartTime).TotalSeconds;

            response["hitCount"] = hitCount;
            response["missCount"] = missCount;
            response["hitRate"] = cacheStats.HitRate();
            response["evictionCount"] = evictionCount;
            response["cacheStartTime"] = cacheStats.StartTime;
            response["hitsPerSecond"] = hitCount/upTime;
            response["missPerSecond"] = missCount/upTime;
            response["requestPerSecond"] = (hitCount + missCount)/upTime;
            response["evictionPerSecond"] = evictionCount/upTime;

            return Ok(response);
        }
    }
}