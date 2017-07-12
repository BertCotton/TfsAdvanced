using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TfsAdvanced.Models.Policy;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.DataStore.Repository
{
    public class PolicyRepository : RepositoryBase<PolicyConfiguration>
    {


        public IEnumerable<PolicyConfiguration> GetByRepository(string repositoryId)
        {
            return base.GetList(p => p.settings.scope != null && p.settings.scope.Any(s => s.repositoryId == repositoryId));
        }

        protected override int GetId(PolicyConfiguration item)
        {
            return item.id;
        }
    }
}
