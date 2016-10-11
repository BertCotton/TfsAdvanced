using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using TfsAdvanced.Data;

namespace TfsAdvanced.Infrastructure
{
    public class AuthenticationTokenProvider
    {
        private readonly HttpContext context;

        public AuthenticationTokenProvider(IHttpContextAccessor context)
        {
            this.context = context.HttpContext;
        }

        public AuthenticationToken GetToken()
        {
            var authCookie = context.Request.Cookies["Auth"];
            return JsonConvert.DeserializeObject<AuthenticationToken>(authCookie);
        }
    }
}
