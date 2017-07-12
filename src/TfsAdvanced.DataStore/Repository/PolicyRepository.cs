using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TfsAdvanced.Models.Policy;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.DataStore.Repository
{
    public class PolicyRepository : RepositoryBase<PolicyConfiguration>
    {

        public PolicyRepository() : base(new PolicyConfigurationComparer())
        {
        }


        public IEnumerable<PolicyConfiguration> GetByRepository(string repositoryId)
        {
            return base.Get(() => data.Where(p => p.settings.scope != null && p.settings.scope.Any(s => s.repositoryId == repositoryId)));
        }
    }

    class PolicyConfigurationComparer : IEqualityComparer<PolicyConfiguration>
    {
        public bool Equals(PolicyConfiguration x, PolicyConfiguration y)
        {
            return x.id == y.id;
        }

        public int GetHashCode(PolicyConfiguration obj)
        {
            return obj.id;
        }
    }
}
