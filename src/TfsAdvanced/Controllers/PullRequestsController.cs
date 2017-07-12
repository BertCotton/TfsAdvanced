using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.DataStore.Repository;
using TfsAdvanced.Models.PullRequests;

namespace TfsAdvanced.Web.Controllers
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
        public IEnumerable<PullRequest> Index()
        {
            return pullRequestRepository.GetAll();
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