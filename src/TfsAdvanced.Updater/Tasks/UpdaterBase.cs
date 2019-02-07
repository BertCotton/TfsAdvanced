using System;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace TFSAdvanced.Updater.Tasks
{
    public abstract class UpdaterBase
    {
        private bool isRunning;

        protected UpdaterBase(ILogger logger)
        {
            Logger = logger;
        }

        protected ILogger Logger { get; }

        [AutomaticRetry(Attempts = 0)]
        public void Run()
        {
            string className = GetType().Name;

            if (isRunning)
            {
                Logger.LogDebug($"Skipping scheduled run for {className} because it is currently running.");
                return;
            }

            isRunning = true;

            Logger.LogInformation($"Starting {className}");
            DateTime start = DateTime.Now;
            try
            {
                Update();
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error running update for {className}.");
            }
            Logger.LogDebug($"Finished Running {className} {DateTime.Now - start:g}");

            isRunning = false;
        }

        protected abstract void Update();
    }
}