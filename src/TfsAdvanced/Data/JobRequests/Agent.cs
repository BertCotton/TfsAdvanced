using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Data.JobRequests
{
    public class Agent
    {
        public int id { get; set; }
        public string name { get; set; }
        public string version { get; set; }
        public bool enabled { get; set; }
        public string status { get; set; }
    }
}
