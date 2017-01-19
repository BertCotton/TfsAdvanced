using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Data.Builds
{
    public class BuildDefinitionLinks
    {
        public HrefLink self { get; set; }

        public HrefLink web { get; set; }

        public HrefLink badge { get; set; }
    }
}
