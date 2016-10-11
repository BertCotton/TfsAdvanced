using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Data.Builds
{
    public class BuildSteps
    {
        public bool enabled { get; set; }

        public bool continueOnError { get; set; }

        public bool alwayRun { get; set; }

        public string displayName { get; set; }

        public int timeoutInMinutes { get; set; }

        public TaskGroupItem task { get; set; }

        public BuildStepInput inputs { get; set; }
    }
}
