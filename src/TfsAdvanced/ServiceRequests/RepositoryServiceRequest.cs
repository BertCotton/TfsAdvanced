using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Errors;
using TfsAdvanced.Data.Projects;
using TfsAdvanced.Data.Repositories;
using TfsAdvanced.Infrastructure;

namespace TfsAdvanced.ServiceRequests
{
    public class RepositoryServiceRequest
    {
        private readonly AppSettings appSettings;
        private readonly ProjectServiceRequest projectServiceRequest;
        private readonly Cache cache;
        private string REPOSITORY_LIST_MEMORY_KEY = "RepositoryListMemoryKey-";
        

        public RepositoryServiceRequest(IOptions<AppSettings> appSettings, Cache cache, ProjectServiceRequest projectServiceRequest)
        {
            this.cache = cache;
            this.projectServiceRequest = projectServiceRequest;
            this.appSettings = appSettings.Value;
        }

        public string BuildDashboardURL(RequestData requestData, Repository repository)
        {
            return $"{requestData.BaseAddress}/{repository.project.name}/_dashboards";
        }

        public async Task<IList<Repository>> GetAllRepositories(RequestData requestData)
        {
            IList<Repository> cached = cache.Get<IList<Repository>>(REPOSITORY_LIST_MEMORY_KEY + "all");
            if (cached != null)
                return cached;

            var exceptions = new ConcurrentQueue<Exception>();
            var concurrentRepositories = new ConcurrentStack<Repository>();
            var projects = await projectServiceRequest.GetProjects(requestData);
            Parallel.ForEach(projects, project =>
            {
                try
                {
                    var repos = GetRepositories(requestData, project).Result;
                    concurrentRepositories.PushRange(repos.ToArray());
                }
                catch (Exception ex)
                {
                    exceptions.Append(ex);
                }
            });
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }

            var repositories = concurrentRepositories.ToList();
            cache.Put(REPOSITORY_LIST_MEMORY_KEY + "all", repositories, TimeSpan.FromHours(1));

            return repositories;
        }

        public async Task<IList<Repository>> GetRepositories(RequestData requestData, Project project)
        {
            IList<Repository> cached = cache.Get<IList<Repository>>(REPOSITORY_LIST_MEMORY_KEY + project.name);
            if (cached != null)
                return cached;

            var url = $"{requestData.BaseAddress}/{project.name}/_apis/git/repositories?api=1.0";
            var response = await requestData.HttpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new BadRequestException(url, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Response<IEnumerable<Repository>> responseObject;
            try
            {
                responseObject = JsonConvert.DeserializeObject<Response<IEnumerable<Repository>>>(responseContent);
            }
            catch (Exception)
            {
                throw new JsonDeserializationException(typeof(Response<IEnumerable<Repository>>), responseContent);
            }
            var repositories = responseObject.value.ToList();
            Parallel.ForEach(repositories, r => r.project.remoteUrl = BuildDashboardURL(requestData, r));

            cache.Put(REPOSITORY_LIST_MEMORY_KEY + project.name, repositories, TimeSpan.FromHours(1));

            return repositories;
        }
    }
}