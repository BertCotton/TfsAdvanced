using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;
using TfsAdvanced.DataStore.Repository;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.Web.SocketConnections
{
    public class PullRequestUpdatesSocket
    {
        private readonly WebSocketClientRepository webSocketClientRepository;
        private readonly PullRequestRepository pullRequestRepository;

        public PullRequestUpdatesSocket(WebSocketClientRepository webSocketClientRepository, PullRequestRepository pullRequestRepository)
        {
            this.webSocketClientRepository = webSocketClientRepository;
            this.pullRequestRepository = pullRequestRepository;
        }

        public async Task RegisterSocket(HttpContext context, WebSocket webSocket)
        {
            int lastId = 0;
            DateTime lastUpdated = DateTime.MinValue;
            while (webSocket.State == WebSocketState.Open)
            {
                var ipAddress = context.Connection.RemoteIpAddress.ToString();
                webSocketClientRepository.UpsertClient(ipAddress);

                IEnumerable<PullRequest> pullRequests = pullRequestRepository.GetPullRequestsAfter(lastId).ToList();
                if (pullRequests != null && pullRequests.Any())
                {

                    var newLastId = pullRequests.OrderByDescending(x => x.Id).Select(x => x.Id).First();
                    if(lastId > 0)
                        await SendNewPullRequests(webSocket, pullRequests);
                    lastId = newLastId;

                }

                var repositoryLastUpdated = pullRequestRepository.GetLastUpdated();

                if (repositoryLastUpdated > lastUpdated)
                {
                    await SendPullRequestList(webSocket, pullRequestRepository.GetAll());
                    lastUpdated = repositoryLastUpdated;
                }

                Thread.Sleep(1000);
            }

        }

        private async Task SendPullRequestList(WebSocket webSocket, IEnumerable<PullRequest> pullRequests)
        {
            try
            {
                IDictionary<string, object> responseObject = new Dictionary<string, object>
                {
                    {"Type", "All"},
                    {"List", pullRequests}
                };
                var response = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseObject));
                await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception e)
            {
                
            }
        }

        private async Task SendNewPullRequests(WebSocket webSocket, IEnumerable<PullRequest> newPullRequests)
        {
            try
            {
                IDictionary<string, object> responseObject = new Dictionary<string, object>
                {
                    {"Type", "Updated"},
                    {"List", newPullRequests}
                };
                var response = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseObject));
                await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception e)
            {

            }
        }
        
        
    }
}
