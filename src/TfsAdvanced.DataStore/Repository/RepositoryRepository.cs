using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.DataStore.Repository
{
    public class RepositoryRepository : RepositoryBase<Models.Repositories.Repository>
    {
        public RepositoryRepository() : base(new RepositoryComparer())
        {

        }
    }

    class RepositoryComparer : IEqualityComparer<Models.Repositories.Repository>
    {
        public bool Equals(Models.Repositories.Repository x, Models.Repositories.Repository y)
        {
            return x.id == y.id;
        }

        public int GetHashCode(Models.Repositories.Repository obj)
        {
            return obj.id.GetHashCode();
        }
    }
}
