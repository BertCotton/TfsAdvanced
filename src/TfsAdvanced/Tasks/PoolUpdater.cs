using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Pools;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Repository;
using TfsAdvanced.Utilities;

namespace TfsAdvanced.Tasks
{
    public class PoolUpdater
    {
        private readonly RequestData requestData;
        private readonly PoolRepository poolRepository;
        private bool IsRunning;

        public PoolUpdater(PoolRepository poolRepository, RequestData requestData)
        {
            this.requestData = requestData;
            this.poolRepository = poolRepository;
        }

        public void Update()
        {
            if (IsRunning)
                return;
            IsRunning = true;
            var pools = GetAsync.FetchResponseList<Pool>(requestData, $"{requestData.BaseAddress}/_apis/distributedtask/pools?api-version=1.0").Result;
            if (pools != null)
                poolRepository.UpdatePools(pools);

            IsRunning = false;
        }
    }
}
