using System;

namespace TfsAdvanced.Models
{
    public class UpdateStatus
    {
        public string UpdaterName { get; set; }
        public DateTime LastUpdate { get; set; }
        public int UpdatedRecords { get; set; }
    }
}
