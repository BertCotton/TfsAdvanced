using System;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task Start()
        {
            
            logger.LogInformation("Starting bootstrapping app");
            DateTime startTime = DateTime.Now;
            // Initialize the updaters in order
            double stepSize = 1.0/9.0;
            double percentLoaded = 0.0;
            var hangFireStatusRepository = serviceProvider.GetService<HangFireStatusRepository>();
            percentLoaded = await InitializeRun<PoolUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = await InitializeRun<ProjectUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = await InitializeRun<RepositoryUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = await InitializeRun<BuildDefinitionUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = await InitializeRun<BuildUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = await InitializeRun<ReleaseDefinitionUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = await InitializeRun<ReleaseUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = await InitializeRun<JobRequestUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
            percentLoaded = await InitializeRun<PullRequestUpdater>(hangFireStatusRepository, percentLoaded, stepSize);
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
                EnqueueJob<ReleaseUpdater>();
                EnqueueJob<ReleaseDefinitionUpdater>();
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            fiveSecondTimer = new Timer(state =>
            {
                EnqueueJob<PullRequestUpdater>();
                EnqueueJob<JobRequestUpdater>();
            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

            logger.LogInformation($"Finished bootstrapping app in {DateTime.Now-startTime:g}");
        }

        private async Task<double> InitializeRun<T>(HangFireStatusRepository hangFireStatusRepository, double percentLoaded, double stepSize) where T : UpdaterBase
        {
            hangFireStatusRepository.SetPercentLoaded(percentLoaded);
            await serviceProvider.GetService<T>().Run(initialize:true);
            return percentLoaded + stepSize;
        }

        private void ScheduleJob<T>(string cron) where T : UpdaterBase
        {
            RecurringJob.AddOrUpdate((T updater) => updater.Run(false), cron);
        }

        private void EnqueueJob<T>() where T : UpdaterBase
        {
            BackgroundJob.Enqueue((T updater) => updater.Run(false));
        }

        public void Stop()
        {
            fiveSecondTimer?.Change(-1, -1);
            thirtySecondTimer?.Change(-1, -1);
        }
        
    }
}
