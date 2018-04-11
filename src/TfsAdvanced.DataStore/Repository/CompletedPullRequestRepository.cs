using System;
using System.Collections.Generic;
using System.Text;
using Redbus;
using Redbus.Interfaces;
using TfsAdvanced.DataStore.Repository;
using TFSAdvanced.DataStore.Interfaces;
using TFSAdvanced.Models.DTO;

namespace TFSAdvanced.DataStore.Repository
{
    public class CompletedPullRequestRepository : PullRequestRepository
    {
        public CompletedPullRequestRepository(IServiceProvider serviceProvider, IEventBus eventBus) : base(serviceProvider, eventBus)
        {
        }
    }
}