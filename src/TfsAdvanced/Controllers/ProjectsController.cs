using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

namespace TfsAdvanced.Web.Controllers
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
        public IEnumerable<Project> GetProjects()
        {
            return projectRepository.GetAll();
        }
    }
}