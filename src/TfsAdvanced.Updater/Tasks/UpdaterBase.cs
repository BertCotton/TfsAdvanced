using System;
using System.Collections.Generic;
using System.Text;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace TFSAdvanced.Updater.Tasks
{
    public abstract class UpdaterBase
    {
        protected readonly ILogger logger;
        private bool isRunning;

        protected UpdaterBase(ILogger logger)
        {
            this.logger = logger;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Run()
        {
            var className = GetType().Name;

            if (isRunning)
            {
                logger.LogDebug($"Skipping scheduled run for {className} because it is currently running.");
                return;
            }

            isRunning = true;

            logger.LogInformation($"Starting {className}");
            var start = DateTime.Now;
            try
            {
                Update();
            }
            catch (Exception e)
            {
                logger.LogError($"Error running update for {className}.", e);
            }
            logger.LogDebug($"Finished Running {className} {DateTime.Now - start:g}");

            isRunning = false;
        }

        protected abstract void Update();
    }
}