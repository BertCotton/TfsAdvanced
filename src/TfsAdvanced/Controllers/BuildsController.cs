using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Builds;
using TfsAdvanced.Repository;
using TfsAdvanced.ServiceRequests;

namespace TfsAdvanced.Controllers
{
    [Route("data/Builds")]
    public class BuildsController : Controller
    {
        private readonly BuildRepository buildRepository;

        public BuildsController(BuildRepository buildRepository)
        {
            this.buildRepository = buildRepository;
        }


        [HttpGet]
        public IList<Build> Index()
        {
            return buildRepository.GetBuilds();
        }

        [HttpGet("statistics")]
        public IEnumerable<DailyBuildStatistic> GetWaitTimes([FromQuery] int NumberOfDaysBack  = 14)
        {
            IEnumerable<Build> builds = buildRepository.GetBuilds();
            if(NumberOfDaysBack > 0)
                builds = builds.Where(b => b.status == BuildStatus.completed && b.startTime.HasValue && b.queueTime > DateTime.Now.AddDays(-NumberOfDaysBack));

            builds = builds.OrderBy(b => b.id);

            Dictionary<DateTime, DailyBuildStatistic> dailyBuildStatistics = new Dictionary<DateTime, DailyBuildStatistic>();

            foreach (var build in builds)
            {
                DateTime day = build.queueTime.Date;
                DailyBuildStatistic statistic = dailyBuildStatistics.ContainsKey(day) ? dailyBuildStatistics[day] : new DailyBuildStatistic(day);
                statistic.AddQueueTime(build.queueTime, build.startTime);
                statistic.AddRunTime(build.startTime, build.finishTime);

                dailyBuildStatistics[day] = statistic;
            }

            return dailyBuildStatistics.Values;
        }
    }
}