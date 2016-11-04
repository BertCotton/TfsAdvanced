using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Data.Builds
{
    public class BuildRun
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public DateTime LaunchedTime { get; set; }

        public int WaitingTime { get; set; }

        public int RunningTime { get; set; }

        public BuildResult BuildResult { get; set; }
    }
}
