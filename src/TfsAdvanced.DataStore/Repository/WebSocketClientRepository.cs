using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TFSAdvanced.DataStore.Repository
{
    public class WebSocketClientRepository
    {
        private readonly IDictionary<string, DateTime> clients;
        private readonly Mutex clientMutex;
        private DateTime lastPurge;

        public WebSocketClientRepository()
        {
            this.clients = new Dictionary<string, DateTime>();
            this.clientMutex = new Mutex(false);
            this.lastPurge = DateTime.Now;
        }

        public void UpsertClient(string clientName)
        {
            if (clientMutex.WaitOne(TimeSpan.FromSeconds(5)))
            {
                try
                {
                    if (clients.ContainsKey(clientName))
                        clients[clientName] = DateTime.Now;
                    else
                        clients.Add(clientName, DateTime.Now);
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
                    foreach (var clientsKey in clients.Keys)
                    {
                        var lastUpdated = clients[clientsKey];
                        if (lastUpdated < purgeTime)
                            clients.Remove(clientsKey);
                    }
                }
                finally
                {
                    clientMutex.ReleaseMutex();
                }
            }
        }
    }
}
