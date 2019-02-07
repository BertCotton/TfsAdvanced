using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Updater.Tasks;

namespace TfsAdvanced.Updater.Tasks
{
    public class PoolUpdater : UpdaterBase
    {
        private readonly PoolRepository poolRepository;
        private readonly RequestData requestData;
        private readonly UpdateStatusRepository updateStatusRepository;

        public PoolUpdater(PoolRepository poolRepository, RequestData requestData, UpdateStatusRepository updateStatusRepository, ILogger<PoolUpdater> logger) : base(logger)
        {
            this.requestData = requestData;
            this.updateStatusRepository = updateStatusRepository;
            this.poolRepository = poolRepository;
        }

        protected override void Update()
        {
            List<Pool> pools = GetAsync.FetchResponseList<Pool>(requestData, $"{requestData.BaseAddress}/_apis/distributedtask/pools?api-version=1.0", Logger).Result;
            if (pools == null) return;

            poolRepository.Update(pools);
            updateStatusRepository.UpdateStatus(new UpdateStatus { LastUpdate = DateTime.Now, UpdatedRecords = pools.Count, UpdaterName = nameof(PoolUpdater) });
        }
    }
}