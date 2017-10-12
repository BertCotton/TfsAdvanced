using System;
using System.Collections.Generic;
using System.Text;

namespace TFSAdvanced.DataStore.Interfaces
{
    public interface IRepository<T>
    {
        IEnumerable<T> GetAll();

        bool Update(IEnumerable<T> updates);

        bool Remove(IEnumerable<T> items);

        DateTime GetLastUpdated();
    }
}
