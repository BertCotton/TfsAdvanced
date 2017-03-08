using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TfsAdvanced.Data;
using Microsoft.EntityFrameworkCore.Storage.Internal;
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
