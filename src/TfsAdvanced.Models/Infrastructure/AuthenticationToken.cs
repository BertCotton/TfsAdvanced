﻿using System;

namespace TfsAdvanced.Models.Infrastructure
{
    public class AuthenticationToken
    {
        public string access_token { get; set; }

        public string token_type { get; set; }

        public int expires_in { get; set; }

        public DateTime expiredTime { get; set; }

        public string refresh_token { get; set; }
    }
}
