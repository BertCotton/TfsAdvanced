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
        private Timer quickUpdateTimer;
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
            double stepSize = 1.0/9.0;
            double percentLoaded = 0.0;
            var hangFireStatusRepository = serviceProvider.GetService<HangFireStatusRepository>();
            percentLoaded = RunUpdate<PoolUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<ProjectUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<RepositoryUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<BuildDefinitionUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<BuildUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<PullRequestUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<CompletedPullRequestUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<ReleaseDefinitionUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = RunUpdate<JobRequestUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            hangFireStatusRepository.SetPercentLoaded(1);

            hangFireStatusRepository.SetIsLoaded(true);

            // Slow moving things only need to be updated once an hour
            ScheduleJob<ProjectUpdater>(Cron.Hourly());
            ScheduleJob<RepositoryUpdater>(Cron.Hourly());
            ScheduleJob<PoolUpdater>(Cron.Hourly());

            ScheduleJob<ReleaseDefinitionUpdater>(Cron.MinuteInterval(30));
            ScheduleJob<BuildDefinitionUpdater>(Cron.MinuteInterval(30));
            
            quickUpdateTimer = new Timer(state =>
            {
                logger.LogInformation("Updating information");
                ExecuteJobs();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            
            logger.LogInformation($"Finished bootstrapping app in {DateTime.Now-startTime:g}");
        }

        private void ExecuteJobs()
        {
            ExecuteJob<BuildUpdater>();
            ExecuteJob<PullRequestUpdater>();
            ExecuteJob<CompletedPullRequestUpdater>();
            ExecuteJob<JobRequestUpdater>();
        }

        private void ExecuteJob<T>() where T : UpdaterBase
        {
            serviceProvider.GetService<T>().Run();
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
            quickUpdateTimer?.Change(-1, -1);
        }
        
    }
}
