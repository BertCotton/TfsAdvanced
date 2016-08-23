using TfsAdvanced.Data;
using TfsAdvanced.ServiceRequests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TfsAdvanced.Controllers
{
    [Route("/data/Projects")]
    public class ProjectsController : Controller
    {

        private readonly TfsRequest tfsRequest;
        private readonly ProjectServiceRequest projectServiceRequests;

        public ProjectsController(TfsRequest tfsRequest, ProjectServiceRequest projectServiceRequests)
        {
            this.tfsRequest = tfsRequest;
            this.projectServiceRequests = projectServiceRequests;
        }

        [HttpGet]
        public async Task<List<Project>> GetProjects()
        {
            using (var requestData = tfsRequest.GetRequestData())
            {
                return await projectServiceRequests.GetProjects(requestData);
            }
        }
    }
}
