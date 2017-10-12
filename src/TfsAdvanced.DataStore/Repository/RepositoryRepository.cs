using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.DataStore.Repository
{
    public class RepositoryRepository : RepositoryBase<TFSAdvanced.Models.DTO.Repository>
    {
        protected override int GetId(TFSAdvanced.Models.DTO.Repository item)
        {
            return item.Id.GetHashCode();
        }

        public TFSAdvanced.Models.DTO.Repository GetById(string Id)
        {
            return Get(x => x.Id == Id);
        }
    }

}
