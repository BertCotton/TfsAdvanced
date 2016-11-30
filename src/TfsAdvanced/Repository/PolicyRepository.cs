using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data.Policy;

namespace TfsAdvanced.Repository
{
    public class PolicyRepository
    {
        public ConcurrentBag<PolicyConfiguration> policies;

        public PolicyRepository()
        {
            policies = new ConcurrentBag<PolicyConfiguration>();
        }

        public IList<PolicyConfiguration> GetPolicies()
        {
            return policies.ToList();
        }

        public IList<PolicyConfiguration> GetByRepository(string repositoryId)
        {
            return policies.Where(p => p.settings.scope != null && p.settings.scope.Any(s => s.repositoryId == repositoryId)).ToList();
        }

        public void SetPolicies(IEnumerable<PolicyConfiguration> policies)
        {
            this.policies = new ConcurrentBag<PolicyConfiguration>(policies);
        }
    }
}
