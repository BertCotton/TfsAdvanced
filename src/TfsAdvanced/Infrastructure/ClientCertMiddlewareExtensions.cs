using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace TfsAdvanced.Infrastructure
{
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ClientCertMiddlewareExtensions
    {
        public static IApplicationBuilder UseClientCertMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClientCertificateMiddleware>();
        }

        public static IApplicationBuilder UseClientCertMiddleware(this IApplicationBuilder builder, IOptions<AppSettings> options)
        {
            return builder.UseMiddleware<ClientCertificateMiddleware>(options);
        }
    }
}
