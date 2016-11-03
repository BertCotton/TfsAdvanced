using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
    }
}