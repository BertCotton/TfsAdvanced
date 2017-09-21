using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TFSAdvanced.Models.DTO;
using TFSAdvanced.Models.Interfaces;

namespace TFSAdvanced.DataStore
{
    public class TfsAdvancedSqlDataContext :  DbContext
    {
        private readonly IConfiguration configuration;

        public TfsAdvancedSqlDataContext(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            //optionsBuilder.UseSqlServer(configuration.GetConnectionString("Local"));
        }


        public DbSet<User> Users { get; set; }

        public DbSet<QueueJob> QueueJobs { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<Models.DTO.Repository> Repositories { get; set; }

        public DbSet<Build> Builds { get; set; }

        public DbSet<BuildDefinition> BuildDefinitions { get; set; }

        public DbSet<ReleaseDefinition> ReleaseDefinitions { get; set; }

        public DbSet<Pool> Pools { get; set; }

        public DbSet<PullRequest> PullRequests { get; set; }

        public DbSet<Reviewer> Reviewers { get; set; }

        public DbSet<Release> Releases { get; set; }

 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            AddIdentity<QueueJob>(modelBuilder, job => job.RequestId);
            AddIdentity<User>(modelBuilder, user => user.UniqueName);
            AddIdentity<Project>(modelBuilder, project => project.ProjectId);
            AddIdentity<Models.DTO.Repository>(modelBuilder, repository => repository.RepositoryId);
            AddIdentity<Build>(modelBuilder);
            AddIdentity<BuildDefinition>(modelBuilder);
            AddIdentity<BuildDefinitionPolicy>(modelBuilder);
            AddIdentity<ReleaseDefinition>(modelBuilder, definition => new { definition.ReleaseDefinitionId, definition.ProjectId} );
            modelBuilder.Entity<ReleaseDefinition>().HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId);
            AddIdentity<Release>(modelBuilder);
            AddIdentity<Pool>(modelBuilder);
            AddIdentity<PullRequest>(modelBuilder);
            AddIdentity<Reviewer>(modelBuilder, reviewer => new {reviewer.PullRequestId, reviewer.UserId});
            base.OnModelCreating(modelBuilder);
        }

        private void AddIdentity<T>(ModelBuilder modelBuilder) where T : class, IIdentity
        {
            modelBuilder.Entity<T>().HasKey(m => m.Id);
            modelBuilder.Entity<T>().Property(m => m.Id).ValueGeneratedNever();
        }

        private void AddIdentity<T>(ModelBuilder modelBuilder, Expression<Func<T, object>> alternateKeyFunc) where T : class, IIdentity
        {
            modelBuilder.Entity<T>().HasKey(m => m.Id);
            modelBuilder.Entity<T>().HasAlternateKey(alternateKeyFunc);
        }

    }
}
