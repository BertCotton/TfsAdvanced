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
        private Timer timer;
        private IServiceProvider serviceProvider;

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

            timer = new Timer(state =>
            {
                Debug.WriteLine("Timer Tick");
                BackgroundJob.Enqueue<BuildDefinitionUpdater>(updater => updater.Update());
                BackgroundJob.Enqueue<BuildUpdater>(updater => updater.Update());
                BackgroundJob.Enqueue<ProjectUpdater>(updater => updater.Update());
                BackgroundJob.Enqueue<PullRequestUpdater>(updater => updater.Update());
                BackgroundJob.Enqueue<RepositoryUpdater>(updater => updater.Update());
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            
        }

        public void Stop()
        {
            if (timer != null)
                timer.Change(-1, -1);
        }
        
    }
}
