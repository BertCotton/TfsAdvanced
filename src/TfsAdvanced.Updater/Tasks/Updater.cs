using System;
using System.Threading;
using System.Timers;
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
            double stepSize = 1.0 / 9.0;
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

            var quickUpdateTimer = new System.Timers.Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
            quickUpdateTimer.Elapsed += ExecuteJobs;
            quickUpdateTimer.AutoReset = true;
            quickUpdateTimer.Enabled = true;

            var pullRequestUpdateTime = new System.Timers.Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
            pullRequestUpdateTime.Elapsed += ExecutePullRequestUpdate;
            pullRequestUpdateTime.AutoReset = true;
            pullRequestUpdateTime.Enabled = true;

            logger.LogInformation($"Finished bootstrapping app in {DateTime.Now - startTime:g}");
        }

        private void ExecutePullRequestUpdate(Object source, ElapsedEventArgs e)
        {
            DateTime startTime = DateTime.Now;
            ExecuteJob<PullRequestUpdater>();
            logger.LogInformation($"Finished Update PullRequests in {DateTime.Now - startTime}");
        }

        private void ExecuteJobs(Object source, ElapsedEventArgs e)
        {
            DateTime startTime = DateTime.Now;
            logger.LogDebug("Executing Update Jobs");
            ExecuteJob<BuildUpdater>();
            ExecuteJob<CompletedPullRequestUpdater>();
            ExecuteJob<JobRequestUpdater>();
            logger.LogInformation($"Finished Update Jobs in {DateTime.Now - startTime}");
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
        }
    }
}