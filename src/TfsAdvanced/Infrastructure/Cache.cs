using Microsoft.Extensions.Caching.Memory;
using System;
using System.Diagnostics;
using TfsAdvanced.Data;

namespace TfsAdvanced.Infrastructure
{
    public class Cache
    {
        private readonly IMemoryCache memoryCache;
        private readonly CacheStats cacheStats;
        private readonly AuthenticationToken authenticationToken;

        public Cache(IMemoryCache memoryCache, CacheStats cacheStats, AuthenticationTokenProvider authenticationTokenProvider)
        {
            this.memoryCache = memoryCache;
            this.cacheStats = cacheStats;
            this.authenticationToken = authenticationTokenProvider.GetToken();
        }

        public T Get<T>(string key)
        {
            T cached;
            if (memoryCache.TryGetValue(buildKey(key), out cached))
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
            memoryCache.Set(buildKey(key), value, new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(cacheTime)
                .RegisterPostEvictionCallback((o, value1, reason, state) =>
                {
                    Debug.WriteLine($"Cache Eviction of {o} because of {reason}");
                    cacheStats.Eviction();
                }));
        }

        private string buildKey(string key)
        {
            return $"{key}";
        }
    }
}