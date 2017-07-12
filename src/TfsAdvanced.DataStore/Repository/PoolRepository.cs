using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TfsAdvanced.Models.Pools;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.DataStore.Repository
{
    public class PoolRepository : RepositoryBase<Pool>
    {
        protected override int GetId(Pool item)
        {
            return item.id;
        }
    }

}
