using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Data.Policy
{
    public class PolicyConfiguration
    {
        public DateTime CreatedDate { get; set; }
        public bool isEnabled { get; set; }
        public bool isBlocking { get; set; }
        public bool isDeleted { get; set; }

        public PolicySetting settings { get; set; }

        public int id { get; set; }

        public string url { get; set; }

        public PolicyType type { get; set; }
    }
}
