using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Serilog;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models.Infrastructure;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.Web.SocketConnections
{
    public class WebSocketUpdater
    {
        private readonly WebSocketClientRepository webSocketClientRepository;
        private readonly PullRequestRepository pullRequestRepository;
        private readonly CompletedPullRequestRepository completedPullRequestRepository;

        private int lastPullRequestId = 0;
        private DateTime lastPullRequestUpdated = DateTime.MinValue;
        DateTime lastCompletedPullRequestUpdated = DateTime.MinValue;

        public WebSocketUpdater(WebSocketClientRepository webSocketClientRepository, PullRequestRepository pullRequestRepository, CompletedPullRequestRepository completedPullRequestRepository)
        {
            this.webSocketClientRepository = webSocketClientRepository;
            this.pullRequestRepository = pullRequestRepository;
            this.completedPullRequestRepository = completedPullRequestRepository;
        }

        public async Task RegisterSocket(HttpContext context, WebSocket webSocket)
        {
            var user = context.User;
            var authToken = JsonConvert.DeserializeObject<AuthenticationToken>(context.Session.GetString("AuthToken"));
            var jwtToken = new JwtSecurityToken(authToken.access_token);

            string currentUserUniqueName = jwtToken.Claims.First(x => x.Type == "unique_name").Value;
            
            while (webSocket.State == WebSocketState.Open)
            {
                var ipAddress = context.Connection.RemoteIpAddress.ToString();
                webSocketClientRepository.UpsertClient(ipAddress);

                await HandleNewPullRequests(webSocket, currentUserUniqueName);
                await HandleUpdatedPullRequests(webSocket, currentUserUniqueName);
                await HandleCompletedPullRequests(webSocket, currentUserUniqueName);
                
                Thread.Sleep(1000);
            }

        }

        private async Task HandleNewPullRequests(WebSocket webSocket, string currentUserUniqueName)
        {
            IEnumerable<PullRequest> pullRequests = pullRequestRepository.GetPullRequestsAfter(lastPullRequestId).ToList();
            if (pullRequests != null && pullRequests.Any())
            {

                var newLastId = pullRequests.OrderByDescending(x => x.Id).Select(x => x.Id).First();
                if (lastPullRequestId > 0)
                {
                    // do not send new pull requests if it was created by the current user
                    await SendNewPullRequests(webSocket, pullRequests.Where(x => x.Creator.UniqueName != currentUserUniqueName).ToList());
                }
                    
                lastPullRequestId = newLastId;

            }
        }

        private async Task HandleUpdatedPullRequests(WebSocket webSocket, string currentUserUniqueName)
        {
            var repositoryLastUpdated = pullRequestRepository.GetLastUpdated();

            if (repositoryLastUpdated > lastPullRequestUpdated)
            {
                var allPullRequests = pullRequestRepository.GetAll().ToList();
                await SendCurrentUserPullRequests(webSocket, allPullRequests.Where(x => x.Creator.UniqueName == currentUserUniqueName).ToList());
                await SendPullRequestList(webSocket, allPullRequests.Where(x => x.Creator.UniqueName != currentUserUniqueName).ToList());
                lastPullRequestUpdated = repositoryLastUpdated;
            }
        }

        private async Task HandleCompletedPullRequests(WebSocket webSocket, string currentUserUniqueName)
        {
            var repositoryLastUpdated = completedPullRequestRepository.GetLastUpdated();

            if (repositoryLastUpdated > lastCompletedPullRequestUpdated)
            {
                await SendPullRequestList(webSocket, completedPullRequestRepository.GetAll());
                lastCompletedPullRequestUpdated = repositoryLastUpdated;
            }
        }

        private async Task SendCurrentUserPullRequests(WebSocket webSocket, IEnumerable<PullRequest> pullRequests)
        {
            try
            {
                IDictionary<string, object> responseObject = new Dictionary<string, object>
                {
                    {"Type", ResponseType.UpdatedCurrentUserPullRequest},
                    {"Data", pullRequests}
                };
                var response = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseObject));
                await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception e)
            {

            }
        }

        private async Task SendPullRequestList(WebSocket webSocket, IEnumerable<PullRequest> pullRequests)
        {
            try
            {
                IDictionary<string, object> responseObject = new Dictionary<string, object>
                {
                    {"Type", ResponseType.UpdatedPullRequest},
                    {"Data", pullRequests}
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
                    {"Type", ResponseType.UpdatedPullRequest},
                    {"Data", newPullRequests}
                };
                var response = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseObject));
                await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception e)
            {

            }
        }

        private async Task SendCompletedPullRequests(WebSocket webSocket, IEnumerable<PullRequest> completedPullRequests)
        {
            try
            {
                IDictionary<string, object> responseObject = new Dictionary<string, object>
                {
                    {"Type", ResponseType.CompletedPullRequest},
                    {"Data", completedPullRequests}
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        
    }
}
