using System.Security.Principal;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TfsAdvanced.Data
{
    public class User : IIdentity
    {
        public string AuthenticationType => token.token_type;

        public bool IsAuthenticated => token.access_token != null;

        public string Name => "User";

        private readonly AuthenticationToken token;

        public User(AuthenticationToken token)
        {
            this.token = token;
        }
    }
}
