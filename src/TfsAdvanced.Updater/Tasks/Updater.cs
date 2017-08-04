using System;
using System.Threading;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using TfsAdvanced.DataStore.Repository;
using TFSAdvanced.Updater.Tasks;

namespace TfsAdvanced.Updater.Tasks
{
    public class Updater
    {
        private Timer fiveSecondTimer;
        private Timer thirtySecondTimer;
        private readonly IServiceProvider serviceProvider;
        
        public Updater(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public void Start()
        {
            // Initialize the updaters in order
            double stepSize = 1.0/7.0;
            double percentLoaded = 0.0;
            var hangFireStatusRepository = serviceProvider.GetService<HangFireStatusRepository>();
            serviceProvider.GetService<ProjectUpdater>().Update();
            percentLoaded += stepSize;
            hangFireStatusRepository.SetPercentLoaded(percentLoaded);
            serviceProvider.GetService<RepositoryUpdater>().Update();
            percentLoaded += stepSize;
            hangFireStatusRepository.SetPercentLoaded(percentLoaded);
            serviceProvider.GetService<BuildUpdater>().Update();
            percentLoaded += stepSize;
            hangFireStatusRepository.SetPercentLoaded(percentLoaded);
            serviceProvider.GetService<BuildDefinitionUpdater>().Update();
            percentLoaded += stepSize;
            hangFireStatusRepository.SetPercentLoaded(percentLoaded);
            serviceProvider.GetService<PullRequestUpdater>().Update();
            percentLoaded += stepSize;
            hangFireStatusRepository.SetPercentLoaded(percentLoaded);
            serviceProvider.GetService<PoolUpdater>().Update();
            percentLoaded += stepSize;
            hangFireStatusRepository.SetPercentLoaded(percentLoaded);
            serviceProvider.GetService<ReleaseDefinitionUpdater>().Update();
            percentLoaded += stepSize;
            hangFireStatusRepository.SetPercentLoaded(percentLoaded);
            serviceProvider.GetService<JobRequestUpdater>().Update();
            percentLoaded += stepSize;
            hangFireStatusRepository.SetPercentLoaded(1);

            hangFireStatusRepository.SetIsLoaded(true);

            // Slow moving things only need to be updated once an hour
            RecurringJob.AddOrUpdate<ProjectUpdater>(updater => updater.Update(), Cron.Hourly);
            RecurringJob.AddOrUpdate<RepositoryUpdater>(updater => updater.Update(), Cron.Hourly);
            RecurringJob.AddOrUpdate<PoolUpdater>(updater => updater.Update(), Cron.Hourly);
       
            thirtySecondTimer = new Timer(state =>
            {
                BackgroundJob.Enqueue<BuildDefinitionUpdater>(updater => updater.Update());
                BackgroundJob.Enqueue<BuildUpdater>(updater => updater.Update());
                BackgroundJob.Enqueue<ReleaseDefinitionUpdater>(updater => updater.Update());
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            fiveSecondTimer = new Timer(state =>
            {
                BackgroundJob.Enqueue<PullRequestUpdater>(updater => updater.Update());
                BackgroundJob.Enqueue<JobRequestUpdater>(updater => updater.Update());
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            
        }

        public void Stop()
        {
            fiveSecondTimer?.Change(-1, -1);
            thirtySecondTimer?.Change(-1, -1);
        }
        
    }
}
