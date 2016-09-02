using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.ServiceRequests;

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