using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TFSAdvanced.DataStore.Interfaces;
using TFSAdvanced.Models.Interfaces;

namespace TFSAdvanced.DataStore.Repository
{
    public abstract class SqlRepositoryBase<T> : IRepository<T>
        where T : class, IIdentity, IUpdateTracked
    {
        
        public IEnumerable<T> GetAll()
        {
            using (var context = new TfsAdvancedSqlDataContext())
            {
                return context.Set<T>();
            }
        }

        public IList<T> GetList(Func<T, bool> d)
        {
            using (var context = new TfsAdvancedSqlDataContext())
            {
                return context.Set<T>().Where(d).ToList();
            }
        }

        public bool Update(IEnumerable<T> updates)
        {
            using (var context = new TfsAdvancedSqlDataContext())
            {
                foreach (var update in updates)
                {
                    update.LastUpdated = DateTime.Now;

                    T data = context.Set<T>().FirstOrDefault(x => x.Id == update.Id);
                    if (data == null)
                        context.Add(update);
                    else
                        Map(update, data);
                }

                var numberOfChanges = context.SaveChanges();

                return numberOfChanges > 0;
            }
        }

        public bool Remove(IEnumerable<T> items)
        {
            bool removed = false;
            using (var context = new TfsAdvancedSqlDataContext())
            {
                foreach (var item in items)
                {
                    T data = context.Set<T>().FirstOrDefault(x => x.Id == item.Id);
                    if (data != null)
                    {
                        context.Remove(data);
                        removed = true;
                    }
                }
                if (removed)
                    context.SaveChanges();
            }
            return removed;
        }

        public DateTime GetLastUpdated()
        {
            using (var context = new TfsAdvancedSqlDataContext())
            {
                return context.Set<T>().Select(x => x.LastUpdated).OrderByDescending(x => x).FirstOrDefault();
            }
        }

        protected abstract void Map(T from, T to);
    }
}
