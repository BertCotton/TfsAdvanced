using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;

namespace TfsAdvanced.Repository
{
    public class UpdateStatusRepository
    {
        private ConcurrentDictionary<string, UpdateStatus> updateStatuses;

        public UpdateStatusRepository()
        {
            updateStatuses = new ConcurrentDictionary<string, UpdateStatus>();
        }

        public List<UpdateStatus> GetStatuses()
        {
            return updateStatuses.Values.OrderBy(x => x.UpdaterName).ToList();
        }

        public void UpdateStatus(UpdateStatus updateStatus)
        {
            updateStatuses.AddOrUpdate(updateStatus.UpdaterName, updateStatus, (s, status) => updateStatus);
        }
    }
}
