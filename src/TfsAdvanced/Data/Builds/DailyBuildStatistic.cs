using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;

namespace TfsAdvanced.Data.Builds
{
    public class DailyBuildStatistic
    {
        public DateTime Day { get; set; }

        private List<double> QueueTimes { get; set; }

        private List<double> RunTimes { get; set; }

        public DailyBuildStatistic(DateTime day)
        {
            Day = day;
            QueueTimes = new List<double>();
            RunTimes = new List<double>();
        }

        public void AddQueueTime(DateTime? start, DateTime? end)
        {
            if (end.HasValue && start.HasValue)
            {
                QueueTimes.Add((end.Value - start.Value).TotalSeconds);
            }
        }

        public void AddRunTime(DateTime? start, DateTime? end)
        {
            if (end.HasValue && start.HasValue)
            {
                RunTimes.Add((end.Value - start.Value).TotalSeconds);
            }
        }

        public double QueueTimeAverage => QueueTimes.Average();

        public double QueueTimeStandardDeviation => QueueTimes.StandardDeviation();

        public double QueueTimesLowerPercentile => QueueTimes.Percentile(25);

        public double QueueTimesUpperPercentile => QueueTimes.Percentile(75);

        public double QueueTimeMin => QueueTimes.Min();

        public double QueueTimeMax => QueueTimes.Max();

        public double RunTimeAverage => RunTimes.Average();

        public double RunTimeStandardDeviation => RunTimes.StandardDeviation();

        public double RunTimesLowerPercentile => RunTimes.Percentile(25);

        public double RunTimesUpperPercentile => RunTimes.Percentile(75);

        public double RunTimesMin => RunTimes.Min();

        public double RunTimesMax => RunTimes.Max();
    }
}
