using System;
using System.Threading;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TfsAdvanced.DataStore.Repository;
using TFSAdvanced.Updater.Tasks;

namespace TfsAdvanced.Updater.Tasks
{
    public class Updater
    {
        private readonly ILogger<Updater> logger;
        private Timer fiveSecondTimer;
        private Timer thirtySecondTimer;
        private readonly IServiceProvider serviceProvider;
        
        public Updater(IServiceProvider serviceProvider, ILogger<Updater> logger)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public void Start()
        {
            
            logger.LogInformation("Starting bootstrapping app");
            DateTime startTime = DateTime.Now;
            // Initialize the updaters in order
            double stepSize = 1.0/8.0;
            double percentLoaded = 0.0;
            var hangFireStatusRepository = serviceProvider.GetService<HangFireStatusRepository>();
            percentLoaded = RunUpdate<ProjectUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<RepositoryUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<BuildUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<BuildDefinitionUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<PullRequestUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<PoolUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<ReleaseDefinitionUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<JobRequestUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            hangFireStatusRepository.SetPercentLoaded(1);

            hangFireStatusRepository.SetIsLoaded(true);

            // Slow moving things only need to be updated once an hour
            ScheduleJob<ProjectUpdater>(Cron.Hourly());
            ScheduleJob<RepositoryUpdater>(Cron.Hourly());
            ScheduleJob<PoolUpdater>(Cron.Hourly());
       
            thirtySecondTimer = new Timer(state =>
            {
                EnqueueJob<BuildDefinitionUpdater>();
                EnqueueJob<BuildUpdater>();
                EnqueueJob<ReleaseDefinitionUpdater>();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            fiveSecondTimer = new Timer(state =>
            {
                EnqueueJob<PullRequestUpdater>();
                EnqueueJob<JobRequestUpdater>();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            
            logger.LogInformation($"Finished bootstrapping app in {DateTime.Now-startTime:g}");
        }

        private double RunUpdate<T>(HangFireStatusRepository hangFireStatusRepository, double percentLoaded, double stepSize) where T : UpdaterBase
        {
            hangFireStatusRepository.SetPercentLoaded(percentLoaded);
            serviceProvider.GetService<T>().Run();
            return percentLoaded + stepSize;
        }

        private void ScheduleJob<T>(string cron) where T : UpdaterBase
        {
            RecurringJob.AddOrUpdate((T updater) => updater.Run(), cron);
        }

        private void EnqueueJob<T>() where T : UpdaterBase
        {
            BackgroundJob.Enqueue((T updater) => updater.Run());
        }

        public void Stop()
        {
            fiveSecondTimer?.Change(-1, -1);
            thirtySecondTimer?.Change(-1, -1);
        }
        
    }
}
