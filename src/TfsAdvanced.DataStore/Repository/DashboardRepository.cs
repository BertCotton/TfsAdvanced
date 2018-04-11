using System;
using System.Collections.Generic;
using System.Text;
using Redbus.Interfaces;
using TFSAdvanced.DataStore.Messages;
using TFSAdvanced.Models.DTO;

namespace TFSAdvanced.DataStore.Repository
{
    public class DashboardRepository : RepositoryBase<Dashboard, DashboardUpdateMessage>
    {
        public DashboardRepository(IServiceProvider serviceProvider, IEventBus eventBus) : base(serviceProvider, eventBus)
        {
        }

        protected override int GetId(Dashboard item)
        {
            return item.Id;
        }
    }
}