using Microsoft.Extensions.Configuration;
using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.DataStore.Repository
{
    public class PoolRepository : SqlRepositoryBase<Pool>
    {
        public PoolRepository(IConfiguration configuration) : base(configuration)
        {
        }

        protected override bool Matches(Pool source, Pool target)
        {
            return source.Id == target.Id;
        }

        protected override void Map(Pool @from, Pool to)
        {
            to.autoProvision = from.autoProvision;
            to.isHosted = from.isHosted;
            to.name = from.name;
            to.size = from.size;
        }
    }

}
