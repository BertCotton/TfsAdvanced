using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.Models
{
    public class WebSocketClient
    {
        public string IpAddress { get; set; }

        public string UniqueName { get; set; }

        public DateTime LastSeen { get; set; }
    }
}
