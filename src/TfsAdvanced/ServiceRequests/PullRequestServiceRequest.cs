using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace TfsAdvanced.ServiceRequests
{
    public class PullRequestServiceRequest
    {
        private readonly AppSettings appSettings;

        public PullRequestServiceRequest(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public string BuildPullRequestUrl(PullRequest pullRequest, string baseUrl)
        {
            return
                $"{baseUrl}/{pullRequest.repository.project.name}/_git/{pullRequest.repository.name}/pullrequest/{pullRequest.pullRequestId}?view=files";
        }

        public IList<PullRequest> GetPullRequests(RequestData requestData, Repository repo, Project project)
        {
            var pullResponse =
                                requestData.HttpClient.GetStringAsync(project._links.pullRequests.href).Result;
            var pullResponseObject = JsonConvert.DeserializeObject<Response<IEnumerable<PullRequest>>>(pullResponse);
            var pullRequests = pullResponseObject.value.ToList();
            pullRequests.ForEach(pr =>
            {
                pr.repository = repo;
                pr.remoteUrl = BuildPullRequestUrl(pr, requestData.BaseAddress);
            });

            return pullRequests;
        }

        public IList<PullRequest> GetPullRequests(RequestData requestData, Dictionary<string, KeyValuePair<Repository, Project>> projects)
        {
            List<PullRequest> pullRequests = new List<PullRequest>();
            projects.Keys.ToList().ForEach(repoId =>
            {
                var pair = projects[repoId];
                var repo = pair.Key;
                var project = pair.Value;
                pullRequests.AddRange(GetPullRequests(requestData, repo, project));
            });
            return pullRequests;
        }
    }
}