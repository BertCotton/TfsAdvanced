using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.Web.SocketConnections
{
    public class PullRequestSocket
    {
        private readonly WebSocketClientRepository webSocketClientRepository;

        public PullRequestSocket(WebSocketClientRepository webSocketClientRepository)
        {
            this.webSocketClientRepository = webSocketClientRepository;
        }

        public async Task RegisterSocket(HttpContext context, WebSocket webSocket)
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var ipAddress = context.Connection.RemoteIpAddress.ToString();
                webSocketClientRepository.UpsertClient(ipAddress);
                var buffer = new byte[1024 * 4];

                DateTime now = DateTime.Now;
                var response = Encoding.UTF8.GetBytes(now.ToString());
                await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
                Thread.Sleep(1000);
            }

        }
        
    }
}
