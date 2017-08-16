using System.Collections.Generic;

namespace TfsAdvanced.Models.Infrastructure
{
    public class AppSettings
    {
        public string BaseAddress { get; set; }

        public string BaseReleaseManagerAddress { get; set; }

        public List<string> Projects { get; set; }

        public Security Security { get; set; }

        public AuthorizationSettings authorization { get; set; }

        public static int MAX_DEGREE_OF_PARALLELISM = 1;
    }
}