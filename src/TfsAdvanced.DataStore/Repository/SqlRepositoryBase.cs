using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TFSAdvanced.DataStore.Interfaces;
using TFSAdvanced.Models.Interfaces;

namespace TFSAdvanced.DataStore.Repository
{
    public abstract class SqlRepositoryBase<T> : IRepository<T>
        where T : class, IIdentity, IUpdateTracked
    {
        protected readonly IConfiguration configuration;

        protected SqlRepositoryBase(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected K GetOrUpdate<K>(TfsAdvancedSqlDataContext dataContext, Expression<Func<K, bool>> filter, K data) where K : class, IUpdateTracked
        {
            if (data == null)
                return null;

            var localRecord = dataContext.Set<K>().Local.FirstOrDefault(filter.Compile());
            if (localRecord != null)
                return localRecord;
            var dbRecord = dataContext.Set<K>().FirstOrDefault(filter);
            if (dbRecord != null)
                return dbRecord;

            data.LastUpdated = DateTime.Now;
            ;
            return data;
        }

        public T Get(Func<T, bool> d)
        {
            using (var context = new TfsAdvancedSqlDataContext(configuration))
            {
                return GetSet(context).FirstOrDefault(d);
            }            
        }
        public virtual IList<T> GetAll()
        {
            using (var context = new TfsAdvancedSqlDataContext(configuration))
            {
                return GetSet(context).ToList();
            }
        }

        public IList<T> GetList(Func<T, bool> d)
        {
            using (var context = new TfsAdvancedSqlDataContext(configuration))
            {
                return GetSet(context).Where(d).ToList();
            }
        }

        public virtual void AttachProperties(TfsAdvancedSqlDataContext dataContext, T t)
        {
            
        }

        protected virtual IQueryable<T> AddIncludes(DbSet<T> data)
        {
            return data;
        }

        private void IncludeProperies(PropertyInfo[] properties, DbSet<T> data, string parentProperties)
        {
            if (properties == null)
                return;
            foreach (var property in properties)
            {
                if (property.SetMethod != null)
                {
                    var propertyIncludeString = $"{parentProperties}.{property.Name}";
                    data.Include(propertyIncludeString);
                    IncludeProperies(property.GetType().GetProperties(), data, propertyIncludeString);
                }
            }
        }

        private IEnumerable<T> GetLocalSet(TfsAdvancedSqlDataContext dataContext)
        {
            return dataContext.Set<T>().Local;
        }

        protected virtual IEnumerable<T> GetSet(TfsAdvancedSqlDataContext dataContext)
        {
            var set = dataContext.Set<T>();
            return AddIncludes(set);
        }
        
        protected abstract bool Matches(T source, T target);

        private T Update(TfsAdvancedSqlDataContext context, T update)
        {
            update.LastUpdated = DateTime.Now;
            // Check the localset
            T data = GetLocalSet(context).FirstOrDefault(x => Matches(x, update));
            // Go to the DB
            if(data == null)
                data = GetSet(context).FirstOrDefault(x => Matches(x, update));

            if (data == null)
            {
                AttachProperties(context, update);
                context.Add(update);
                data = update;
            }
            else
            {
                Map(update, data);
                AttachProperties(context, data);
            }
            return data;
        }


        public async Task<bool> Update(T update)
        {
            using (var context = new TfsAdvancedSqlDataContext(configuration))
            {
                var data = Update(context, update);
                var numberOfChanges = await context.SaveChangesAsync();
                update.Id = data.Id;
                return numberOfChanges == 1;
            }
        }

     public async Task<bool> Update(IList<T> updates)
        {
            int numberOfChanges = 0;
            using (var context = new TfsAdvancedSqlDataContext(configuration))
            {
                foreach (var update in updates)
                {
                    var data = Update(context, update);
                    update.Id = data.Id;
                }

                numberOfChanges = await context.SaveChangesAsync();
                
            }

            using (var context = new TfsAdvancedSqlDataContext(configuration))
            {
                foreach (var update in updates)
                {
                    T data = GetSet(context).FirstOrDefault(x => Matches(x, update));
                    if(data != null)
                        update.Id = data.Id;
                }
            }
            return numberOfChanges > 0;
        }

        public async Task<bool> Remove(IEnumerable<T> items)
        {
            bool removed = false;
            using (var context = new TfsAdvancedSqlDataContext(configuration))
            {
                foreach (var item in items)
                {
                    T data = GetSet(context).FirstOrDefault(x => Matches(x, item));
                    if (data != null)
                    {
                        context.Remove(data);
                        removed = true;
                    }
                }
                if (removed)
                    await context.SaveChangesAsync();
            }
            return removed;
        }

        public virtual DateTime GetLastUpdated()
        {
            using (var context = new TfsAdvancedSqlDataContext(configuration))
            {
                return context.Set<T>().Select(x => x.LastUpdated).OrderByDescending(x => x).FirstOrDefault();
            }
        }

        public bool IsEmpty()
        {
            using (var context = new TfsAdvancedSqlDataContext(configuration))
            {
                return !context.Set<T>().Any();
            }
        }

        protected abstract void Map(T from, T to);
    }
}
