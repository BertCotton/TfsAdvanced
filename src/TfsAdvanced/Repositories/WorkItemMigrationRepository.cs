using System.Linq;
using TfsAdvanced.Data;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.OptionsModel;

namespace TfsAdvanced.Infrastructure
{
    public class WorkItemMigrationRepository
    {
        private AppSettings appSettings;
        private IMemoryCache memoryCache;
        private static string MEM_KEY = "MIGRATION_WORK_ID-";

        public WorkItemMigrationRepository(IOptions<AppSettings> appSettings, IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
            this.appSettings = appSettings.Value;
        }

        public int? GetMigratedWorkId(int originalId)
        {
            int? migratedWorkId;
            memoryCache.TryGetValue(MEM_KEY + originalId, out migratedWorkId);
            if(migratedWorkId.HasValue)
                return migratedWorkId;

            using (var context = new DataContext(appSettings))
            {
                var migration = context.WorkItemMigrations.FirstOrDefault(x => x.OriginalId == originalId);
                if (migration != null && migration.MigratedId > 0)
                {
                    migratedWorkId = migration.MigratedId;
                    memoryCache.Set(MEM_KEY + originalId, migratedWorkId);
                    return migratedWorkId;
                }
                    
            }
            return null;
        }

        public void SetMigrations(WorkItemMigration migration)
        {
            using (var context = new DataContext(appSettings))
            {
                context.WorkItemMigrations.Add(migration);
                context.SaveChanges();
            }
        }
    }
}