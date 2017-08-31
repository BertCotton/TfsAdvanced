using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using TFSAdvanced.DataStore.Interfaces;

namespace TFSAdvanced.DataStore.Repository
{
    public abstract class RepositoryBase<T> : IRepository<T>
    {
        protected readonly Dictionary<int, T> data;
        protected readonly Mutex mutex;
        protected DateTime LastUpdated;
        protected DateTime LastCleanup;
        

        protected RepositoryBase()
        {
            this.data = new Dictionary<int, T>();
            this.mutex = new Mutex();
            this.LastUpdated = DateTime.Now;
            this.LastCleanup = DateTime.Now;
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


        public virtual bool Update(IEnumerable<T> updates)
        {
            var updated = false;
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
                        {
                            T existing = data[id];
                            if (!existing.Equals(update))
                            {
                                data[id] = update;
                                updated = true;
                            }
                            
                        }

                        else
                        {
                            data.Add(id, update);
                            updated = true;
                        }
                    }

                    if (updated)
                        LastUpdated = DateTime.Now;
                }
                finally

                {
                    mutex.ReleaseMutex();
                }
            }
            return updated;
        }

        public bool Remove(IEnumerable<T> items)
        {
            var removed = false;
            if (mutex.WaitOne(60))
            {
                try
                {

                    // ToList to prevent removing key if it is an enumeration of data
                    foreach (var item in items.ToList())
                    {
                        var key = GetId(item);
                        if (data.ContainsKey(key))
                        {
                            data.Remove(key);
                            removed = true;
                        }
                    }

                    if(removed)
                        LastUpdated = DateTime.Now;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return removed;
        }

        public DateTime GetLastUpdated()
        {
            if (mutex.WaitOne(TimeSpan.FromSeconds(3)))
            {
                try
                {
                    return LastUpdated;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            return DateTime.MinValue;
        }

        protected void CleanupIfNeeded(Predicate<T> removePredicate)
        {
            // Only run cleanup every 3 hours
            if (LastUpdated.AddHours(3) > DateTime.Now)
            {
                Cleanup(removePredicate);
            }
        }

        private void Cleanup(Predicate<T> removePredicate)
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
                    LastCleanup = DateTime.Now;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
}
