using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using TfsAdvanced.Data;
using TfsAdvanced.Data.PullRequests;
using TfsAdvanced.ServiceRequests;

namespace TfsAdvanced.Controllers
{
    [Route("data/PullRequests")]
    public class PullRequestsController : Controller
    {
        private readonly ProjectServiceRequest projectServiceRequest;
        private readonly PullRequestServiceRequest pullRequestServiceRequest;
        private readonly RepositoryServiceRequest repositoryServiceRequest;
        private readonly RequestData requestData;

        public PullRequestsController(RequestData requestData, ProjectServiceRequest projectServiceRequest, PullRequestServiceRequest pullRequestServiceRequest, RepositoryServiceRequest repositoryServiceRequest)
        {
            this.requestData = requestData;
            this.projectServiceRequest = projectServiceRequest;
            this.pullRequestServiceRequest = pullRequestServiceRequest;
            this.repositoryServiceRequest = repositoryServiceRequest;
        }

        [HttpGet]
        public IList<PullRequest> Index()
        {
            var repositories = repositoryServiceRequest.GetAllRepositories(requestData);
            var projects = projectServiceRequest.GetProjects(requestData, repositories);
            return pullRequestServiceRequest.GetPullRequests(requestData, projects);
        }
    }
}