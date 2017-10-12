using System;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.DataStore.Repository;

namespace TfsAdvanced.Web.Controllers
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
                toDate = DateTime.Now.AddMinutes(10);
            if (!fromtDate.HasValue)
                fromtDate = toDate.Value.AddDays(-2);
            return Ok(jobRequestRepository.GetJobRequests(fromtDate.Value, toDate.Value));
        }
    }
}
