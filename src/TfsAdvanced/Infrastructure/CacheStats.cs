using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TfsAdvanced.Infrastructure
{
    public class CacheStats
    {
        private int hitCount;
        private int missCount;
        private int evictionCount;
        
        public void Hit()
        {
            Interlocked.Increment(ref hitCount);
        }

        public void Miss()
        {
            Interlocked.Increment(ref missCount);
        }

        public void Eviction()
        {
            Interlocked.Increment(ref evictionCount);
        }

        public int HitCount()
        {
            return hitCount;
        }

        public int MissCount()
        {
            return missCount;
        }

        public int EvictionCount()
        {
            return evictionCount;
        }

        public double HitRate()
        {
            return (hitCount + 0d)/(missCount + hitCount);
        }
    }
}
