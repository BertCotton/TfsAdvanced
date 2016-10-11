using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TfsAdvanced.Data
{
    public class AuthenticationToken
    {
        public string access_token { get; set; }

        public string base64_token => Convert.ToBase64String(Encoding.ASCII.GetBytes(access_token));

        public string token_type { get; set; }

        public int expires_in { get; set; }

        public string refresh_token { get; set; }
    }
}
