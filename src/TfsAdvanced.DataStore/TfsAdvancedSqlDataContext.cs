using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Models.Interfaces;

namespace TFSAdvanced.DataStore
{
    public class TfsAdvancedSqlDataContext :  DbContext
    {
        public DbSet<QueueJob> QueueJobs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TfsAdvanced;Trusted_Connection=True;MultipleActiveResultSets=true");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            AddIdentity<QueueJob>(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private void AddIdentity<T>(ModelBuilder modelBuilder) where T : class, IIdentity
        {
            modelBuilder.Entity<T>().HasKey(m => m.Id);
        }
        
    }
}
