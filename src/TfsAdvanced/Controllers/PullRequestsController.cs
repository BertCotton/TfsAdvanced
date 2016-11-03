using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TfsAdvanced.Data;
using TfsAdvanced.Data.PullRequests;
using TfsAdvanced.Repository;
using TfsAdvanced.ServiceRequests;

namespace TfsAdvanced.Controllers
{
    [Route("data/PullRequests")]
    public class PullRequestsController : Controller
    {
        private readonly PullRequestRepository pullRequestRepository;

        public PullRequestsController(PullRequestRepository pullRequestRepository)
        {
            this.pullRequestRepository = pullRequestRepository;
        }

        [HttpGet]
        public IList<PullRequest> Index()
        {
            return pullRequestRepository.GetPullRequests();
        }
    }
}