using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using TfsAdvanced.Models.Infrastructure;

namespace TfsAdvanced.Infrastructure
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;

            if (path.Value.StartsWith("/data/Login"))
            {
                await _next.Invoke(context);
                return;
            }

            //if (context.Request.Host.Value.Contains("localhost"))
            //{
            //    await _next.Invoke(context);
            //    return;
            //}
            
            byte[] value;
            if (context.Session.TryGetValue("AuthToken", out value))
            {
                var token = JsonConvert.DeserializeObject<AuthenticationToken>(ASCIIEncoding.ASCII.GetString(value));
                if (token?.access_token == null)
                {
                    context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                    await context.Response.WriteAsync("Unable to read session.  User is not authenticated.");
                    return;
                }
                if (DateTime.Now > token.expiredTime)
                {
                    context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Token is expired.  User needs to reauthenticate.");
                    return;
                }
                await _next.Invoke(context);
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync("Session does not have AuthToken set.  User is not authenticated.");
        }
    }
}
