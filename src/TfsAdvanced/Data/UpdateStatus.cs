using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Data
{
    public class UpdateStatus
    {
        public string UpdaterName { get; set; }
        public DateTime LastUpdate { get; set; }
        public int UpdatedRecords { get; set; }
    }
}
