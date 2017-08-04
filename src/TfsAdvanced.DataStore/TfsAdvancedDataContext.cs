using Microsoft.EntityFrameworkCore;
using TfsAdvanced.Models.Infrastructure;

namespace TfsAdvanced.Data
{
    public class TfsAdvancedDataContext : DbContext
    {
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseInMemoryDatabase();
            base.OnConfiguring(options);
        }
    }
}
