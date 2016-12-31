using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.Repository;

namespace TfsAdvanced.Controllers
{
    [Route("data/JobRequests")]
    public class JobRequestsController : Controller
    {
        private readonly JobRequestRepository jobRequestRepository;

        public JobRequestsController(JobRequestRepository jobRequestRepository)
        {
            this.jobRequestRepository = jobRequestRepository;
        }

        [HttpGet]
        public IActionResult GetJobRequests([FromQuery] DateTime? fromtDate, [FromQuery] DateTime? toDate )
        {
            if (!toDate.HasValue)
                toDate = DateTime.Now;
            if (!fromtDate.HasValue)
                fromtDate = toDate.Value.AddDays(-2);
            return Ok(jobRequestRepository.GetJobRequests(fromtDate.Value, toDate.Value));
        }
    }
}
