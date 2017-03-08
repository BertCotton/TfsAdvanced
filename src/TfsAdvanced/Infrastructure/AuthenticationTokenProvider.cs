using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using TfsAdvanced.Data;
using TfsAdvanced.Models.Infrastructure;

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
            byte[] value;
            if (context.Session.TryGetValue("AuthToken", out value))
            {
                var token = JsonConvert.DeserializeObject<AuthenticationToken>(ASCIIEncoding.ASCII.GetString(value));
                return token;
            }
            throw new Exception("Unable to get token from session.");

        }
    }
}
