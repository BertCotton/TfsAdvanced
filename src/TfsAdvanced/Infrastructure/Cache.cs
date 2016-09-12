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

        public T Get<T>(string key)
        {
            T cached;
            if (memoryCache.TryGetValue(key, out cached))
            {
                Debug.WriteLine($"Cache Hit [{key}]");
                return cached;
            }
            Debug.WriteLine($"Cache Miss [{key}]");
            return default(T);
        }

        public void Put(string key, object value, TimeSpan cacheTime)
        {
            Debug.WriteLine($"Cache set [{key}] for {cacheTime}");
            memoryCache.Set(key, value, cacheTime);
        }
    }
}
