using System;

namespace TfsAdvanced.Models.Pools
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
