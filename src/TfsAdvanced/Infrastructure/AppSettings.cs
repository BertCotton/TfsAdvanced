using System.Collections.Generic;

namespace TfsAdvanced.Infrastructure
{
    public class AppSettings
    {
        public string BaseAddress { get; set; }
        public AuthorizationSettings authorization { get; set; }
    }
}