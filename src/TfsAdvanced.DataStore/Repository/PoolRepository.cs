using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TfsAdvanced.Models.Pools;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.DataStore.Repository
{
    public class PoolRepository : RepositoryBase<Pool>
    {
        public PoolRepository() : base(new PoolComparer())
        {
        }
    }

    class PoolComparer : IEqualityComparer<Pool>
    {
        public bool Equals(Pool x, Pool y)
        {
            return x.id == y.id;
        }

        public int GetHashCode(Pool obj)
        {
            return obj.id;
        }
    }
}
