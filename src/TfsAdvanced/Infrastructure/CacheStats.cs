using System;
using System.Threading;

namespace TfsAdvanced.Infrastructure
{
    public class CacheStats
    {
        private int userLogins;
        private int hitCount;
        private int missCount;
        private int evictionCount;
        public readonly DateTime StartTime = DateTime.Now;

        public void UserLogin()
        {
            Interlocked.Increment(ref userLogins);
        }
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

        public int UserLoginCount()
        {
            return userLogins;
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
            return (hitCount + 0d) / (missCount + hitCount);
        }
    }
}