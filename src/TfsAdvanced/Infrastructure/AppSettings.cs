using System.Collections.Generic;

namespace TfsAdvanced.Infrastructure
{
    public class AppSettings
    {
        public string BaseAddress { get; set; }

        public List<string> Projects { get; set; }

        public Security Security { get; set; }

    public AuthorizationSettings authorization { get; set; }
    }
}