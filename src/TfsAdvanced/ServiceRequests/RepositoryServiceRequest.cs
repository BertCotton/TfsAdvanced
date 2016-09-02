using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TfsAdvanced.Data;
using TfsAdvanced.Data.Repositories;
using TfsAdvanced.Infrastructure;

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