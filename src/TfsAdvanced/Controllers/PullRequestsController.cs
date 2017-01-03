using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet("newcheck/{sinceId}")]
        public IList<Dictionary<string, string>> NewPullRequestSince([FromRoute] int sinceId)
        {
            var pullRequests = pullRequestRepository.GetPullRequestsAfter(sinceId);
            return pullRequests.Select(pullRequest => new Dictionary<string, string>
            {
                {"author", pullRequest.createdBy.displayName },
                {"id", pullRequest.pullRequestId.ToString()},
                {"repository", pullRequest.repository.name},
                {"title", pullRequest.title},
                {"url", pullRequest.remoteUrl}
            }).ToList();
        }
    }
}