using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Data.Policy
{
    public class PolicyScope
    {
        public string refName { get; set; }

        public MatchKind[] scope { get; set; }

        public string repositoryId { get; set; }

    }
}
