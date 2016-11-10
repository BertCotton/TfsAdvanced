﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data.Pools;

namespace TfsAdvanced.Repository
{
    public class PoolRepository
    {
        private ConcurrentStack<Pool> pools;

        public PoolRepository()
        {
            this.pools = new ConcurrentStack<Pool>();
        }

        public IList<Pool> GetPools()
        {
            return pools.ToList();
        }

        public void UpdatePools(IList<Pool> pools)
        {
            this.pools = new ConcurrentStack<Pool>(pools);
        }
    }
}
