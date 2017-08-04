using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using TfsAdvanced.Models.Infrastructure;

namespace TfsAdvanced.Infrastructure
{
    public static class AuthenticationMiddlewareExtension
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>();
        }

        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder, IOptions<AppSettings> options)
        {
            return builder.UseMiddleware<AuthenticationMiddleware>(options);
        }
    }
}
