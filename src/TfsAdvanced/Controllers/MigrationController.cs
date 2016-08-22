using TfsAdvanced.Data;
using TfsAdvanced.ServiceRequests;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TfsAdvanced.Controllers
{
    [Route("data/Migrations")]
    public class MigrationController : Controller
    {
        private const string CACHE_KEY = "pullRequests";

        private static List<string> blacklisted = new List<string>
            {
                "Benefits",
                "WorkHistoryManagement"
            };

        private readonly IMemoryCache memoryCache;
        private readonly ProjectServiceRequest projectServiceRequest;
        private readonly PullRequestServiceRequest pullRequestServiceRequest;
        private readonly RepositoryServiceRequest repositoryServiceRequest;
        private readonly TfsRequest tfsRequest;

        public MigrationController(IMemoryCache memoryCache,
            ProjectServiceRequest projectServiceRequest, PullRequestServiceRequest pullRequestServiceRequest,
            RepositoryServiceRequest repositoryServiceRequest, TfsRequest tfsRequest)
        {
            this.memoryCache = memoryCache;
            this.projectServiceRequest = projectServiceRequest;
            this.pullRequestServiceRequest = pullRequestServiceRequest;
            this.repositoryServiceRequest = repositoryServiceRequest;
            this.tfsRequest = tfsRequest;
        }

        [HttpGet("CheckExists")]
        public IList<RepositoryExistsCheck> CheckExistsRepositories([FromQuery] string ProjectName)
        {
            List<Repository> repositories = new List<Repository>();
            using (var httpClient = tfsRequest.GetHttpClient())
            {
                var remoteRepositories =
                    repositoryServiceRequest.GetAllRepositories(httpClient)
                        .Where(r => !blacklisted.Contains(r.name))
                        .ToList();

                if (ProjectName != null)
                    repositories.AddRange(remoteRepositories.Where(r => r.project.name == ProjectName).ToList());
            }

            using (var httpClient = tfsRequest.GetCloudHttpClient())
            {
                IList<RepositoryExistsCheck> checks = new List<RepositoryExistsCheck>();
                repositories.ForEach(r =>
                {
                    checks.Add(
                        repositoryServiceRequest.DoesRepositoryExist(httpClient, r)
                            .Result);
                });
                return checks;
            }
        }

        [HttpGet("Create")]
        public IList<Repository> CreateRepositories([FromQuery] string ProjectName)
        {
            var repositories = CheckExistsRepositories(ProjectName).Where(r => r.exists == false).ToList();

            IList<Repository> createdRepositories = new List<Repository>(repositories.Count());
            using (var httpClient = tfsRequest.GetCloudHttpClient())
            {
                repositories.ForEach(r =>
                {
                    createdRepositories.Add(repositoryServiceRequest.CreateRepository(httpClient, r).Result);
                });
            }
            return createdRepositories;
        }

        [HttpGet]
        [Produces("application/binary")]
        public IActionResult GetMigrationScripts([FromQuery] string ProjectName)
        {
            using (var requestData = tfsRequest.GetHttpClient())
            {
                var repositories = repositoryServiceRequest.GetAllRepositories(requestData).Where(r => !blacklisted.Contains(r.name))
                        .ToList();

                if (ProjectName != null)
                    repositories = repositories.Where(r => r.project.name == ProjectName).ToList();

                var name = "Migrate-" + ProjectName + ".sh";
                FileInfo info = new FileInfo(name);
                using (StreamWriter writer = info.CreateText())
                {
                    repositories.ForEach(r =>
                    {
                        writer.WriteLine(
                            $"rm -fr {r.name} && git clone {r.remoteUrl} && cd {r.name} &&  git remote add new-origin ssh://iusdev@iusdev.visualstudio.com:22/{r.project.name}/_git/{r.name} && git push new-origin --all && git push new-origin +refs/remotes/origin/*:refs/heads/* && git push new-origin --tags && cd ../;");
                    });
                }

                return File(info.OpenRead(), "text/plain");
            }
        }
    }
}