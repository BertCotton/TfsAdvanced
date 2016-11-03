using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Repository
{
    public class RepositoryRepository
    {
        private ConcurrentBag<Data.Repositories.Repository> repositories;

        public RepositoryRepository()
        {
            repositories = new ConcurrentBag<Data.Repositories.Repository>();
        }

        public IList<Data.Repositories.Repository> GetRepositories()
        {
            return repositories.ToImmutableList();
        }

        public void UpdateRepositories(IList<Data.Repositories.Repository> repositories)
        {
            this.repositories = new ConcurrentBag<Data.Repositories.Repository>(repositories);
        }
    }
}
