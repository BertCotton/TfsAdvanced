using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
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
            try
            {
                if (mutex.WaitOne(TimeSpan.FromSeconds(5)))
                    return data.Values;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return new List<T>();
        }

        protected T Get(Predicate<T> d)
        {
            try
            {
                if (mutex.WaitOne(TimeSpan.FromSeconds(5)))
                    return data.Values.FirstOrDefault(x => d(x));
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return default(T);
        }

        protected IEnumerable<T> GetList(Predicate<T> d)
        {
            try
            {
                if (mutex.WaitOne(TimeSpan.FromSeconds(5)))
                    return data.Values.Where(x => d(x));
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return new List<T>();
        }


        public virtual void Update(IEnumerable<T> updates)
        {
            try
            {
                if (mutex.WaitOne(TimeSpan.FromSeconds(60)))
                {
                    foreach (T update in updates)
                    {
                        var id = GetId(update);
                        if (data.ContainsKey(id))
                            data[id] = update;
                        else
                            data.Add(id, update);
                    }
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public void Remove(IEnumerable<T> items)
        {
            try
            {
                if (mutex.WaitOne(60))
                {
                    foreach (var item in items)
                    {
                        var key = GetId(item);
                        if(data.ContainsKey(key))
                            data.Remove(key);
                    }
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        protected void Cleanup(Predicate<T> removePredicate)
        {
            try
            {
                if (mutex.WaitOne(60))
                {
                    var itemsToRemove = data.Values.ToImmutableList().Where(x => removePredicate(x));
                    foreach (var item in itemsToRemove)
                    {
                        var key = GetId(item);
                        data.Remove(key);
                    }
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}
