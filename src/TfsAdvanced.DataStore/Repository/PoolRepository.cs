using System;
using Redbus.Interfaces;
using TFSAdvanced.DataStore.Messages;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class PoolRepository : RepositoryBase<Pool, PoolUpdateMessage>
    {
        public PoolRepository(IServiceProvider serviceProvider, IEventBus eventBus) : base(serviceProvider, eventBus)
        {
        }

        protected override int GetId(Pool item)
        {
            return item.id;
        }
    }
}