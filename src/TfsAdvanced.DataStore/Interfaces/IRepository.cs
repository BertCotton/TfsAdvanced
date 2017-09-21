using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TFSAdvanced.DataStore.Interfaces
{
    public interface IRepository<T>
    {
        IList<T> GetAll();

        Task<bool> Update(T update);

        Task<bool> Update(IList<T> updates);

        Task<bool> Remove(IEnumerable<T> items);

        DateTime GetLastUpdated();
    }
}
