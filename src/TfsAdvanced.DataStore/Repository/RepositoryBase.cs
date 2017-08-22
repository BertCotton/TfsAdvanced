using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace TFSAdvanced.DataStore.Repository
{
    public abstract class RepositoryBase<T>
    {
        protected readonly Dictionary<int, T> data;
        protected readonly Mutex mutex;

        protected RepositoryBase()
        {
            this.data = new Dictionary<int, T>();
            this.mutex = new Mutex();
        }

        protected abstract int GetId(T item);

        public IEnumerable<T> GetAll()
        {
            if (mutex.WaitOne(TimeSpan.FromSeconds(5)))
            {
                try
                {
                    return data.Values;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return new List<T>();
        }

        protected T Get(Predicate<T> d)
        {
            if (mutex.WaitOne(TimeSpan.FromSeconds(5)))
            {
                try
                {

                    return data.Values.FirstOrDefault(x => d(x));
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return default(T);
        }

        protected IEnumerable<T> GetList(Predicate<T> d)
        {
            if (mutex.WaitOne(TimeSpan.FromSeconds(5)))
            {
                try
                {

                    return data.Values.Where(x => d(x));
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return new List<T>();
        }


        public virtual void Update(IEnumerable<T> updates)
        {
            if (mutex.WaitOne(TimeSpan.FromSeconds(60)))
            {
                try
                {

                    foreach (T update in updates)
                    {
                        if (update == null)
                            continue;
                        var id = GetId(update);
                        if (data.ContainsKey(id))
                            data[id] = update;
                        else
                            data.Add(id, update);
                    }
                }
                finally

                {
                    mutex.ReleaseMutex();
                }
            }
        }

        public void Remove(IEnumerable<T> items)
        {
            if (mutex.WaitOne(60))
            {
                try
                {

                    // ToList to prevent removing key if it is an enumeration of data
                    foreach (var item in items.ToList())
                    {
                        var key = GetId(item);
                        if (data.ContainsKey(key))
                            data.Remove(key);
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        protected void Cleanup(Predicate<T> removePredicate)
        {
            if (mutex.WaitOne(60))
            {
                try
                {
                    var itemsToRemove = data.Values.ToImmutableList().Where(x => removePredicate(x)).ToList();
                    foreach (var item in itemsToRemove)
                    {
                        var key = GetId(item);
                        data.Remove(key);
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}
