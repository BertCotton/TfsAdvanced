using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Infrastructure
{
    public class AuthorizationSettings
    {
        public string ClientId { get; set; }

        public string State { get; set; }

        public string Scope { get; set; }

        public string RedirectURI { get; set; }

        public string AppSecret { get; set; }
    }
}
