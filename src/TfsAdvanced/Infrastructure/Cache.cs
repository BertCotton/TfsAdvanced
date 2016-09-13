using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace TfsAdvanced.Infrastructure
{
    public class Cache
    {
        private readonly IMemoryCache memoryCache;
        private readonly CacheStats cacheStats;

        public Cache(IMemoryCache memoryCache, CacheStats cacheStats)
        {
            this.memoryCache = memoryCache;
            this.cacheStats = cacheStats;
        }

        public T Get<T>(string key)
        {
            T cached;
            if (memoryCache.TryGetValue(key, out cached))
            {
                Debug.WriteLine($"Cache Hit [{key}]");
                cacheStats.Hit();
                return cached;
            }

            Debug.WriteLine($"Cache Miss [{key}]");
            cacheStats.Miss();
            return default(T);
        }

        public void Put(string key, object value, TimeSpan cacheTime)
        {
            Debug.WriteLine($"Cache set [{key}] for {cacheTime}");
            memoryCache.Set(key, value, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(cacheTime)
                .RegisterPostEvictionCallback((o, value1, reason, state) =>
                {
                    Debug.WriteLine($"Cache Eviction of {o} because of {reason}");
                    cacheStats.Eviction();
                }));
        }
    }
}
