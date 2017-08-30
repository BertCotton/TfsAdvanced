using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TFSAdvanced.Models;

namespace TFSAdvanced.DataStore.Repository
{
    public class WebSocketClientRepository
    {
        private readonly IList<WebSocketClient> clients;
        private readonly Mutex clientMutex;
        private DateTime lastPurge;

        public WebSocketClientRepository()
        {
            this.clients = new List<WebSocketClient>();
            this.clientMutex = new Mutex(false);
            this.lastPurge = DateTime.Now;
        }

        public IList<WebSocketClient> GetClients()
        {
            if (clientMutex.WaitOne(TimeSpan.FromSeconds(3)))
            {
                try
                {
                    return clients.ToList();
                }
                finally
                {
                    clientMutex.ReleaseMutex();
                }
            }
            return new List<WebSocketClient>();
        }

        public void UpsertClient(WebSocketClient updatedClient)
        {
            if (clientMutex.WaitOne(TimeSpan.FromSeconds(5)))
            {
                try
                {
                    foreach (var client in clients)
                    {
                        if (client.UniqueName == updatedClient.UniqueName && client.IpAddress == updatedClient.IpAddress)
                        {
                            client.LastSeen = updatedClient.LastSeen;
                            return;
                        }
                    }
                    clients.Add(updatedClient);
                }
                finally
                {
                    clientMutex.ReleaseMutex();
                }
            }

            if(lastPurge.AddHours(3) < DateTime.Now)
                CleanUp();
        }

        private void CleanUp()
        {
            if (clientMutex.WaitOne(TimeSpan.FromSeconds(5)))
            {
                var purgeTime = DateTime.Now.AddHours(-1);
                try
                {
                    IList<WebSocketClient> clientsToRemove = new List<WebSocketClient>();
                    foreach (var client in clients)
                    {
                        if (client.LastSeen< purgeTime)
                            clientsToRemove.Add(client);
                    }

                    foreach (var client in clientsToRemove)
                    {
                        clients.Remove(client);
                    }
                }
                finally
                {
                    clientMutex.ReleaseMutex();
                }
            }
        }

        public void RemoveClient(WebSocketClient webSocketClient)
        {
            if (clientMutex.WaitOne(TimeSpan.FromSeconds(5)))
            {
                try
                {

                    WebSocketClient clientToRemove = clients.FirstOrDefault(x => x.UniqueName == webSocketClient.UniqueName && x.IpAddress == webSocketClient.IpAddress);
                    if (clientToRemove != null)
                        clients.Remove(clientToRemove);

                }
                finally
                {
                    clientMutex.ReleaseMutex();
                }
            }
        }
    }
}
