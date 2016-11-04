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

        [HttpGet("WaitTimes")]
        public IList<BuildRun> GetWaitTimes([FromQuery] int NumberOfDaysBack  = 14)
        {
            return buildRepository.GetBuilds()
                .Where(b => b.status == BuildStatus.completed && b.startTime.HasValue && b.queueTime > DateTime.Now.AddDays(-NumberOfDaysBack))
                .OrderBy(b => b.id)
                .Select(b => new BuildRun
                {
                    Id = b.id,
                    LaunchedTime = b.queueTime,
                    Url = b._links.web.href,
                    WaitingTime = Convert.ToInt32((b.startTime.Value - b.queueTime).TotalSeconds)
                })
                .ToList();
        }
    }
}