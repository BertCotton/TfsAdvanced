using TFSAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

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
