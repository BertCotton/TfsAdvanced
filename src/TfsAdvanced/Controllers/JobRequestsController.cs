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
        public IActionResult GetJobRequests()
        {
            return Ok(jobRequestRepository.GetJobRequests());
        }
    }
}
