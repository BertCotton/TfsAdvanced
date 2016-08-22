using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using TfsAdvanced.Data;

namespace TfsAdvanced.Infrastructure
{
    public class DataContext : DbContext
    {
        public DataContext(AppSettings appSettings) : base(appSettings.DatabaseConnection)
        {
            Database.Initialize(false);
            Database.Log = null;
        }

        public DbSet<WorkItemMigration> WorkItemMigrations { get; set; }

        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkItemMigration>().HasKey(x => x.OriginalId);

            modelBuilder.Entity<WorkItemMigration>().Property(x => x.OriginalId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}