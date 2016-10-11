using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Data.Builds
{
    public class TaskGroupItem
    {
        public string id { get; set; }
        public string versionSpec { get; set; }
        public string definitionType { get; set; }
    }
}
