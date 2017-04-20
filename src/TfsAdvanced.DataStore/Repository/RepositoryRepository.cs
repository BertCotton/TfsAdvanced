using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TfsAdvanced.DataStore.Repository
{
    public class RepositoryRepository
    {
        private ConcurrentBag<Models.Repositories.Repository> repositories;

        public RepositoryRepository()
        {
            repositories = new ConcurrentBag<Models.Repositories.Repository>();
        }

        public IList<Models.Repositories.Repository> GetRepositories()
        {
            return repositories.ToImmutableList();
        }

        public void UpdateRepositories(IList<Models.Repositories.Repository> repositories)
        {
            this.repositories = new ConcurrentBag<Models.Repositories.Repository>(repositories);
        }
    }
}
