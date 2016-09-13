using System;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Data.PullRequests;
using TfsAdvanced.Data.Repositories;
using TfsAdvanced.Infrastructure;

namespace TfsAdvanced.ServiceRequests
{
    public class PullRequestServiceRequest
    {
        private readonly AppSettings appSettings;
        private readonly Cache cache;
        private string MEMORY_REPOSITORY_KEY = "MEMORY_REPOSITORY_KEY-";

        public PullRequestServiceRequest(IOptions<AppSettings> appSettings, Cache cache)
        {
            this.cache = cache;
            this.appSettings = appSettings.Value;
        }

        public string BuildPullRequestUrl(PullRequest pullRequest, string baseUrl)
        {
            return
                $"{baseUrl}/{pullRequest.repository.project.name}/_git/{pullRequest.repository.name}/pullrequest/{pullRequest.pullRequestId}?view=files";
        }

        public async Task<IList<PullRequest>> GetPullRequests(RequestData requestData, Repository repo, Project project)
        {
            string cacheKey = MEMORY_REPOSITORY_KEY + repo.id + project.id;
            IList<PullRequest> cached = cache.Get<IList<PullRequest>>(cacheKey);
            if(cached != null)
                return cached;
            var pullResponse = await requestData.HttpClient.GetStringAsync(project._links.pullRequests.href);
            var pullResponseObject = JsonConvert.DeserializeObject<Response<IEnumerable<PullRequest>>>(pullResponse);
            var pullRequests = pullResponseObject.value.ToList();
            Parallel.ForEach(pullRequests, pr =>
            {
                pr.repository = repo;
                pr.remoteUrl = BuildPullRequestUrl(pr, requestData.BaseAddress);
            });

            cache.Put(cacheKey, pullRequests, TimeSpan.FromSeconds(20));

            return pullRequests;
        }

        public IList<PullRequest> GetPullRequests(RequestData requestData, Dictionary<string, KeyValuePair<Repository, Project>> projects)
        {
            var pullRequests = new ConcurrentStack<PullRequest>();
            var repoIds = projects.Keys.ToList();
            Parallel.ForEach(repoIds, repoId =>
            {
                var pair = projects[repoId];
                var repo = pair.Key;
                var project = pair.Value;
                var projectPullRequests = GetPullRequests(requestData, repo, project).Result;
                if (projectPullRequests.Any())
                    pullRequests.PushRange(projectPullRequests.ToArray());
            });
            return pullRequests.ToList();
        }
    }
}