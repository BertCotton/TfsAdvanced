using System.Collections.Concurrent;
using TfsAdvanced.Data;
using TfsAdvanced.Infrastructure;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TfsAdvanced.ServiceRequests
{
    public class RepositoryServiceRequest
    {
        private readonly AppSettings appSettings;

        public RepositoryServiceRequest(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings.Value;
        }

        public string BuildDashboardURL(RequestData requestData, Repository repository)
        {
            return $"{requestData.BaseAddress}/{repository.project.name}/_dashboards";
        }

        public async Task<Repository> CreateRepository(RequestData requestData, RepositoryExistsCheck repositoryExistsCheck)
        {
            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{requestData.BaseAddress}/{repositoryExistsCheck.projectName}/_apis/git/repositories?api-version=2.2");
            request.Content = new StringContent(
                JsonConvert.SerializeObject(new CreateRepositoryRequest
                {
                    name = repositoryExistsCheck.name,
                    id = new ProjectGuid { id = repositoryExistsCheck.projectId }
                }), Encoding.UTF8,
                "application/json");
            var buildResponse = await requestData.HttpClient.SendAsync(request);
            var responseString = await buildResponse.Content.ReadAsStringAsync();
            var repository = JsonConvert.DeserializeObject<Repository>(responseString);
            if (repository.name == null)
            {
                repository.name = repositoryExistsCheck.name;
                repository.id = responseString;
            }
            return repository;
        }

        public async Task<RepositoryExistsCheck> DoesRepositoryExist(RequestData requestData, Repository repository)
        {
            var url = $"{requestData.BaseAddress}/{repository.project.name}/_apis/git/repositories/{repository.id}?api=2.2";

            var response = await requestData.HttpClient.GetAsync(url);
            return new RepositoryExistsCheck
            {
                name = repository.name,
                url = url,
                exists = (response.StatusCode != HttpStatusCode.NotFound),
                projectName = repository.project.name,
                projectId = repository.project.id
            };
        }

        public IList<Repository> GetAllRepositories(RequestData requestData)
        {
            var repositories = new ConcurrentStack<Repository>();
            Parallel.ForEach(appSettings.Projects, project =>
            {
                var repos = GetRepositories(requestData, project).Result;
                repositories.PushRange(repos.ToArray());
            });
            return repositories.ToList();
        }

        public async Task<IList<Repository>> GetRepositories(RequestData requestData, string tfsProject)
        {
            var response = await requestData.HttpClient.GetStringAsync($"{requestData.BaseAddress}/{tfsProject}/_apis/git/repositories?api=1.0");
            var responseObject = JsonConvert.DeserializeObject<Response<IEnumerable<Repository>>>(response);
            var repositories = responseObject.value.ToList();
            Parallel.ForEach(repositories, r => r.project.remoteUrl = BuildDashboardURL(requestData, r));

            return repositories;
        }
    }
}