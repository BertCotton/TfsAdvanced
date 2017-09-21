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
using TFSAdvanced.Models;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.Web.SocketConnections
{
    public class WebSocketUpdater
    {
        private readonly WebSocketClientRepository webSocketClientRepository;
        private readonly PullRequestRepository pullRequestRepository;
        private readonly CompletedPullRequestRepository completedPullRequestRepository;
        private readonly HangFireStatusRepository hangFireStatusRepository;
        private readonly ILogger logger;
        
        private int lastPullRequestId = 0;
        private DateTime lastPullRequestUpdated = DateTime.MinValue;
        DateTime lastCompletedPullRequestUpdated = DateTime.MinValue;

        public WebSocketUpdater(WebSocketClientRepository webSocketClientRepository, PullRequestRepository pullRequestRepository, CompletedPullRequestRepository completedPullRequestRepository, HangFireStatusRepository hangFireStatusRepository)
        {
            this.webSocketClientRepository = webSocketClientRepository;
            this.pullRequestRepository = pullRequestRepository;
            this.completedPullRequestRepository = completedPullRequestRepository;
            this.hangFireStatusRepository = hangFireStatusRepository;
            this.logger = Log.Logger;
        }

        public async Task RegisterSocket(HttpContext context, WebSocket webSocket)
        {
            var user = context.User;
            var authToken = JsonConvert.DeserializeObject<AuthenticationToken>(context.Session.GetString("AuthToken"));
            var jwtToken = new JwtSecurityToken(authToken.access_token);

            string currentUserUniqueName = jwtToken.Claims.First(x => x.Type == "unique_name").Value;
            var ipAddress = context.Connection.RemoteIpAddress.ToString();

            while (webSocket.State == WebSocketState.Open)
            {
                
                webSocketClientRepository.UpsertClient(new WebSocketClient
                {
                    IpAddress = ipAddress,
                    UniqueName = currentUserUniqueName,
                    LastSeen = DateTime.Now
                });

                if (hangFireStatusRepository.IsLoaded())
                {
                    await HandleNewPullRequests(webSocket, currentUserUniqueName);
                    await HandleUpdatedPullRequests(webSocket, currentUserUniqueName);
                    await HandleCompletedPullRequests(webSocket, currentUserUniqueName);
                }
                Thread.Sleep(1000);
            }

            webSocketClientRepository.RemoveClient(new WebSocketClient
            {
                IpAddress = ipAddress,
                UniqueName = currentUserUniqueName,
                LastSeen = DateTime.Now
            });

        }

        private async Task HandleNewPullRequests(WebSocket webSocket, string currentUserUniqueName)
        {
            IList<PullRequest> pullRequests = pullRequestRepository.GetPullRequestsAfter(lastPullRequestId);
            if (pullRequests != null && pullRequests.Any())
            {

                var newLastId = pullRequests.OrderByDescending(x => x.Id).Select(x => x.Id).First();
                if (lastPullRequestId > 0)
                {
                    // do not send new pull requests if it was created by the current user
                    await SendNewPullRequests(webSocket, pullRequests.Where(x => x.Creator.UniqueName != currentUserUniqueName));
                }
                    
                lastPullRequestId = newLastId;

            }
        }

        private async Task SendNewPullRequests(WebSocket webSocket, IEnumerable<PullRequest> newPullRequests)
        {
            IDictionary<string, object> responseObject = new Dictionary<string, object>
            {
                {"Type", ResponseType.NewPullRequest},
                {"Data", newPullRequests}
            };
            await SendMessage(webSocket, responseObject);
        }

        private async Task HandleUpdatedPullRequests(WebSocket webSocket, string currentUserUniqueName)
        {
            var repositoryLastUpdated = pullRequestRepository.GetLastUpdated();

            if (repositoryLastUpdated > lastPullRequestUpdated)
            {
                var allPullRequests = pullRequestRepository.GetAll();

                await SendCurrentUserPullRequests(webSocket, allPullRequests.Where(x => x.Creator.UniqueName == currentUserUniqueName));
                await SendPullRequestList(webSocket, allPullRequests.Where(x => x.Creator.UniqueName != currentUserUniqueName));
                lastPullRequestUpdated = repositoryLastUpdated;
            }
        }

        private async Task SendCurrentUserPullRequests(WebSocket webSocket, IEnumerable<PullRequest> pullRequests)
        {
            IDictionary<string, object> responseObject = new Dictionary<string, object>
            {
                {"Type", ResponseType.UpdatedCurrentUserPullRequest},
                {"Data", pullRequests}
            };
            await SendMessage(webSocket, responseObject);
        }

        private async Task SendPullRequestList(WebSocket webSocket, IEnumerable<PullRequest> pullRequests)
        {
            IDictionary<string, object> responseObject = new Dictionary<string, object>
            {
                {"Type", ResponseType.UpdatedPullRequest},
                {"Data", pullRequests}
            };
            await SendMessage(webSocket, responseObject);
        }

        private async Task HandleCompletedPullRequests(WebSocket webSocket, string currentUserUniqueName)
        {   
            var currentUserCompletedMessages = completedPullRequestRepository.GetForUser(currentUserUniqueName);
            if (!currentUserCompletedMessages.Any())
                return;

            var repositoryLastUpdated = currentUserCompletedMessages.OrderByDescending(x => x.ClosedDate).First().ClosedDate;
            if (!repositoryLastUpdated.HasValue)
                return;

            if (repositoryLastUpdated > lastCompletedPullRequestUpdated)
            {
                await SendCompletedPullRequests(webSocket, currentUserCompletedMessages, ResponseType.CurrentUserCompletedPullRequest);
                // If this is the first time loading, don't send all the completed ones
                if (lastCompletedPullRequestUpdated > DateTime.MinValue)
                {
                    var newlyCompleted = currentUserCompletedMessages.Where(x => x.ClosedDate.HasValue && x.ClosedDate.Value > lastCompletedPullRequestUpdated);
                    await SendCompletedPullRequests(webSocket, newlyCompleted, ResponseType.NewCurrentUserCompletedPullRequest);
                }
                lastCompletedPullRequestUpdated = repositoryLastUpdated.Value;
            }
        }

        
        private async Task SendMessage(WebSocket webSocket, IDictionary<string, object> message)
        {
            try
            {
                var response = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                await webSocket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.Error("Error sending message to client", ex);
            }
        }


        private async Task SendCompletedPullRequests(WebSocket webSocket, IEnumerable<PullRequest> completedPullRequests, ResponseType responseType)
        {
            IDictionary<string, object> responseObject = new Dictionary<string, object>
            {
                {"Type", responseType},
                {"Data", completedPullRequests}
            };
            await SendMessage(webSocket, responseObject);
        }


    }
}
