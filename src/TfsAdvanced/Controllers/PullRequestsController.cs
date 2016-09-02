using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;
using TfsAdvanced.ServiceRequests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TfsAdvanced.Controllers
{
    [Route("data/PullRequests")]
    public class PullRequestsController : Controller
    {
        private const string CACHE_KEY = "pullRequests";

        private readonly IMemoryCache memoryCache;
        private readonly ProjectServiceRequest projectServiceRequest;
        private readonly PullRequestServiceRequest pullRequestServiceRequest;
        private readonly RepositoryServiceRequest repositoryServiceRequest;
        private readonly RequestData requestData;

        public PullRequestsController(RequestData requestData, ProjectServiceRequest projectServiceRequest, PullRequestServiceRequest pullRequestServiceRequest, RepositoryServiceRequest repositoryServiceRequest, IMemoryCache memoryCache)
        {
            this.requestData = requestData;
            this.projectServiceRequest = projectServiceRequest;
            this.pullRequestServiceRequest = pullRequestServiceRequest;
            this.repositoryServiceRequest = repositoryServiceRequest;
            this.memoryCache = memoryCache;
        }

        [HttpGet]
        public IList<PullRequest> Index()
        {
            IList<PullRequest> cachedModel;
            if (memoryCache.TryGetValue(CACHE_KEY, out cachedModel))
                return cachedModel;

            var repositories = repositoryServiceRequest.GetAllRepositories(requestData);
            var projects = projectServiceRequest.GetProjects(requestData, repositories);
            var pullRequest = pullRequestServiceRequest.GetPullRequests(requestData, projects);

            memoryCache.Set(CACHE_KEY, pullRequest,
                new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(10)));

            return pullRequest;

        }
    }
}