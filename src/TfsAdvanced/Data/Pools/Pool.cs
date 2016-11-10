using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Data.Pools
{
    public class Pool
    {
        public int size { get; set; }
        public DateTime createdOn { get; set; }
        public bool autoProvision { get; set; }
        public bool isHosted { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }
}
