using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TfsAdvanced.DataStore.Repository;
using TFSAdvanced.Models.DTO;

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
                {"author", pullRequest.Creator.Name },
                {"id", pullRequest.Id.ToString()},
                {"repository", pullRequest.Repository.Name},
                {"title", pullRequest.Title},
                {"url", pullRequest.Url}
            }).ToList();
        }
    }
}