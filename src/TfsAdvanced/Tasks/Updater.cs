using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace TfsAdvanced.Tasks
{
    public class Updater
    {
        private Timer tenSecondTimer;
        private readonly IServiceProvider serviceProvider;
        
        public Updater(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Start()
        {
            // Initialize the updaters in order
            serviceProvider.GetService<ProjectUpdater>().Update();
            serviceProvider.GetService<RepositoryUpdater>().Update();
            serviceProvider.GetService<BuildUpdater>().Update();
            serviceProvider.GetService<BuildDefinitionUpdater>().Update();
            serviceProvider.GetService<PullRequestUpdater>().Update();
            serviceProvider.GetService<PoolUpdater>().Update();
            serviceProvider.GetService<JobRequestUpdater>().Update();

            // Slow moving things only need to be updated once an hour
            RecurringJob.AddOrUpdate<ProjectUpdater>(updater => updater.Update(), Cron.Hourly);
            RecurringJob.AddOrUpdate<RepositoryUpdater>(updater => updater.Update(), Cron.Hourly);
            RecurringJob.AddOrUpdate<PoolUpdater>(updater => updater.Update(), Cron.Hourly);
            RecurringJob.AddOrUpdate<BuildDefinitionUpdater>(updater => updater.Update(), Cron.Hourly);


            tenSecondTimer = new Timer(state =>
            {
                BackgroundJob.Enqueue<BuildUpdater>(updater => updater.Update());
                BackgroundJob.Enqueue<PullRequestUpdater>(updater => updater.Update());
                BackgroundJob.Enqueue<JobRequestUpdater>(updater => updater.Update());
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            
        }

        public void Stop()
        {
            if (tenSecondTimer != null)
                tenSecondTimer.Change(-1, -1);
        }
        
    }
}