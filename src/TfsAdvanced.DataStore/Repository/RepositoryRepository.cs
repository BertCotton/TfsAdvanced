using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.DataStore.Repository
{
    public class RepositoryRepository : RepositoryBase<Models.Repositories.Repository>
    {
        protected override int GetId(Models.Repositories.Repository item)
        {
            return item.id.GetHashCode();
        }
    }

}
