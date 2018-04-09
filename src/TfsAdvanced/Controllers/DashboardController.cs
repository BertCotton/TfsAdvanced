using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.DataStore.Repository;
using TFSAdvanced.DataStore.Repository;

namespace TfsAdvanced.Web.Controllers
{
    [Route("data/Dashboard")]
    public class DashboardController : Controller
    {
        private readonly BuildRepository buildRepository;
        private readonly JobRequestRepository jobRequestRepository;

        public DashboardController(BuildRepository buildRepository, JobRequestRepository jobRequestRepository)
        {
            this.buildRepository = buildRepository;
            this.jobRequestRepository = jobRequestRepository;
        }

        [HttpGet]
        public IActionResult GetDashboards()
        {
            var latestBuilds = buildRepository.GetLatestPerRepository();

            var failedBuilds = latestBuilds.Where(x => x.BuildStatus != TFSAdvanced.Models.DTO.BuildStatus.Succeeded);

            var latestReleases = jobRequestRepository.GetLatestPerRepository();

            var failedReleases = latestReleases.Where(x => x.JobType == TFSAdvanced.Models.DTO.JobType.Release && x.QueueJobStatus != TFSAdvanced.Models.DTO.QueueJobStatus.Succeeded);

            return Ok(new Dictionary<string, object>
            {
                {"builds", failedBuilds },
                {"releases", failedReleases }
            });
        }
    }
}