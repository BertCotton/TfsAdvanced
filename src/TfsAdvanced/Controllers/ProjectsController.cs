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

        private readonly RequestData requestData;
        private readonly ProjectServiceRequest projectServiceRequests;

        public ProjectsController(RequestData requestData, ProjectServiceRequest projectServiceRequests)
        {
            this.requestData = requestData;
            this.projectServiceRequests = projectServiceRequests;
        }

        [HttpGet]
        public async Task<List<Project>> GetProjects()
        {
            return await projectServiceRequests.GetProjects(requestData);

        }
    }
}
