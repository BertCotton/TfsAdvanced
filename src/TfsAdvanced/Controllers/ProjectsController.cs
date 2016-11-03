using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Repository;
using TfsAdvanced.ServiceRequests;

namespace TfsAdvanced.Controllers
{
    [Route("/data/Projects")]
    public class ProjectsController : Controller
    {
        private readonly ProjectRepository projectRepository;

        public ProjectsController(ProjectRepository projectRepository)
        {
            this.projectRepository = projectRepository;
        }


        [HttpGet]
        public IList<Project> GetProjects()
        {
            return projectRepository.GetProjects();
        }
    }
}