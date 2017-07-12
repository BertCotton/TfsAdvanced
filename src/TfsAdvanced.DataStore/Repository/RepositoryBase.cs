using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TFSAdvanced.DataStore.Repository
{
    public abstract class RepositoryBase<T>
    {
        protected readonly HashSet<T> data;
        protected readonly Mutex mutex;

        protected RepositoryBase(IEqualityComparer<T> comparer)
        {
            this.data = new HashSet<T>(comparer);
            this.mutex = new Mutex();
        }

        public IEnumerable<T> GetAll()
        {
            try
            {
                if (mutex.WaitOne(TimeSpan.FromSeconds(5)))
                    return data;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return new List<T>();
        }

        protected T Get(Func<T> d)
        {
            try
            {
                if (mutex.WaitOne(TimeSpan.FromSeconds(5)))
                    return d();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return default(T);
        }

        protected IEnumerable<T> Get(Func<IEnumerable<T>> d)
        {
            try
            {
                if (mutex.WaitOne(TimeSpan.FromSeconds(5)))
                    return d();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            return new List<T>();
        }


        public void Update(IEnumerable<T> updates)
        {
            try
            {
                if (mutex.WaitOne(TimeSpan.FromSeconds(60)))
                {
                    foreach (T update in updates)
                    {
                        this.data.Add(update);
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
